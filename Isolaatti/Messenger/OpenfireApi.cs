using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Isolaatti.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Isolaatti.Messenger;

public class Property
{
    public string Key { get; set; }
    public string Value { get; set; }
}
public class CreateUserDto
{
    public string Username { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<Property> Properties { get; set; }
}
public class OpenfireApi(HttpClientSingleton httpClient, IOptions<MessengerConfig> options, ILogger<OpenfireApi> logger) : IMessagingServer
{
    public async Task<bool> CreateUser(int userId, string username, string name, string email, string password)
    {
        logger.LogDebug(options.Value.ApiKey);
        var dto = new CreateUserDto()
        {
            Email = email,
            Name = name,
            Username = username,
            Password = password,
            Properties = [
                new Property()
                {
                    Key = "Isolaatti.UserId",
                    Value = userId.ToString()
                }
            ]
        };
        try
        {
            var jsonContent = JsonContent.Create(dto);
            var url = new Uri($"{options.Value.OpenfireApiUrl}/v1/users");
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Headers =
                {
                    { "Authorization", options.Value.ApiKey },
                    { "Accept", "application/xml" }
                },
                Content = jsonContent
            };

            var response = await httpClient.Client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Created)
            {
                logger.LogInformation("Account created at openfire for user: \"{}\"", username);
                return true;
            }
            logger.LogError("Error creating account for user \"{}\": status code: {}, url: {} response string: \"{}\"", 
                username, response.StatusCode, url.ToString(), await response.Content.ReadAsStringAsync());
            return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }
}
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
    public string Key;
    public string Value;
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
    public async Task<bool> CreateUser(string username, string name, string email, string password)
    {
        logger.LogDebug(options.Value.ApiKey);
        var dto = new CreateUserDto()
        {
            Email = email,
            Name = name,
            Username = username,
            Password = password,
            Properties = []
        };
        try
        {
            var jsonContent = JsonContent.Create(dto);
            logger.LogDebug(JsonSerializer.Serialize(dto));
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{options.Value.OpenfireApiUrl}/v1/users"))
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
                return true;
            }
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
        

        return true;
    }
}
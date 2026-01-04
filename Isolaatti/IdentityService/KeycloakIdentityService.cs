using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Isolaatti.Common;
using Isolaatti.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Isolaatti.IdentityService;

public class KeycloakIdentityService(IOptions<Keycloak> keycloakOptions, HttpClientSingleton httpClientSingleton, ILogger<KeycloakIdentityService> logger) : IIdentityService
{

    private async Task<TokenResponse?> GetAccessToken()
    {
        var client = httpClientSingleton.Client;
        var uriBuilder = new UriBuilder(keycloakOptions.Value.AdminApi)
        {
            Path = $"/realms/{keycloakOptions.Value.Realm}/protocol/openid-connect/token"
        };
        
        var credential = keycloakOptions.Value.AdminClientId + ":" + keycloakOptions.Value.AdminClientSecret;
        var credentialBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(credential));

        var request = new HttpRequestMessage()
        {
            RequestUri = uriBuilder.Uri,
            Method = HttpMethod.Post,
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Basic", credentialBase64)
            },
            Content = new FormUrlEncodedContent([(new KeyValuePair<string, string>("grant_type", "client_credentials"))])
        };

        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadAsStringAsync();
            
            

            return JsonSerializer.Deserialize<TokenResponse>(token);
        }
        else
        {
            logger.LogError("Failed to get access token from Keycloak. status code = {}, body = {}", response.StatusCode, await response.Content.ReadAsStringAsync());
            return null;
        }


        

    }
    
    public async Task<bool> UserExists(string email)
    {
        var uriBuilder = new UriBuilder(keycloakOptions.Value.AdminApi)
        {
            Path = $"/admin/realms/{keycloakOptions.Value.Realm}/users/count",
            Query = $"username={email}"
        };

        var client = httpClientSingleton.Client;
        
        var token = await GetAccessToken();

        if (token == null)
        {
            throw new Exception("Failed to get access token from Keycloak");
        }

        var request = new HttpRequestMessage()
        {
            RequestUri = uriBuilder.Uri,
            Method = HttpMethod.Get,
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken)
            }
        };

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var count = await response.Content.ReadAsAsync<int>();
        return count > 0;
    }

    public async Task<string> CreateUser(string username, string email, string password, string firstName, string lastName)
    {
        var uriBuilder = new UriBuilder(keycloakOptions.Value.AdminApi)
        {
            Path = $"/admin/realms/{keycloakOptions.Value.Realm}/users"
        };

        var client = httpClientSingleton.Client;
        var newUser = new UserRepresentation()
        {
            Username = username,
            Email = email,
            Enabled = true,
            EmailVerified = true,
            FirstName = firstName,
            LastName = lastName,
            Credentials = [new Credential() { Value = password, Type = "password" }]
        };
        var token = await GetAccessToken();

        if (token == null)
        {
            throw new Exception("Failed to get access token from Keycloak");
        }
        

        var request = new HttpRequestMessage()
        {
            RequestUri = uriBuilder.Uri,
            Method = HttpMethod.Post,
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", (token).AccessToken)
            },
            Content = JsonContent.Create(newUser)
        };

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        
        var userId = response.Headers.Location?.Segments.Last();

        return userId ?? throw new Exception("Failed to create user");

    }
}
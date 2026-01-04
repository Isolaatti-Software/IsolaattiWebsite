using System.Text.Json.Serialization;

namespace Isolaatti.IdentityService;

public class TokenResponse
{
    [JsonPropertyName( "access_token")]
    public string AccessToken { get; set; }
}
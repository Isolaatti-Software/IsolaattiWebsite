using System.Text.Json;
using Isolaatti.Models.MongoDB;

namespace Isolaatti.DTOs;

public class SessionDto
{
    public string SessionId {get;set;}
    public string SessionKey {get;set;}

    public static SessionDto? FromJson(string? json)
    {
        if (json == null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<SessionDto>(json);
        }
        catch (JsonException)
        {
            return null;
        }
        
    }
}
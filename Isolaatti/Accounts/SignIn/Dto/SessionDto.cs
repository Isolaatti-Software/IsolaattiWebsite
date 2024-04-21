using System;
using System.Text.Json;

namespace Isolaatti.Accounts.SignIn.Dto;

public class SessionDto
{
    public Guid SessionId {get;set;}
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
using System;

namespace Isolaatti.Classes.Authentication;

public class AuthenticationTokenSerializable
{
    public string Id { get; set; }
    public string Guid { get; set; }
    public string Secret { get; set; }

    public override string ToString()
    {
        return $"{Id}.{Guid}.{Secret}";
    }

    /// <summary>
    /// Returns an AuthenticationTokenSerializable object from a string
    /// </summary>
    /// <param name="stringToken">The token string</param>
    /// <returns>AuthenticationTokenSerializable object</returns>
    /// <exception cref="FormatException">Token string does not have correct format</exception>
    public static AuthenticationTokenSerializable FromString(string stringToken)
    {
        var splited = stringToken.Split(".");
        if (splited.Length != 3)
        {
            throw new FormatException("Token does not have correct format. It must be id.guid.secret");
        }
        

        return new AuthenticationTokenSerializable
        {
            Id = splited[0],
            Guid = splited[1],
            Secret = splited[2]
        };
    }
}
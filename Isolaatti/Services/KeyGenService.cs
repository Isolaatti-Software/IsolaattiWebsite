using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace Isolaatti.Services;

public class KeyGenService
{
    /// <summary>
    /// Generates a 64 byte random key and hash. Hashed key is generated using the PasswordHasher class from Microsoft.AspNetCore.Identity
    /// </summary>
    /// <returns>Item1: guid used as user param of PasswordHasher, Item2: key base64 encoded, Item3: hashedKey</returns>
    public Tuple<string, string> GenerateKeyAndHash()
    {
        var randomData = new byte[64];
        RandomNumberGenerator.Create().GetBytes(randomData);
        var guid = Guid.NewGuid();
        var key = Convert.ToBase64String(randomData);

        return new Tuple<string, string>(guid.ToString(), key);
    }
}
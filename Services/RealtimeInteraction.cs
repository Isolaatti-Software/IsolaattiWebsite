using Isolaatti.Utils;
using Microsoft.AspNetCore.Identity;

namespace Isolaatti.Services;

public struct NewServerKeyRegister
{
    public string name, key;

    public NewServerKeyRegister(string keyName, string keyValue)
    {
        name = keyName;
        key = keyValue;
    }
}

public class RealtimeInteraction
{
    public NewServerKeyRegister RegisterNewServerKey()
    {
        var keyName = RandomData.GenerateRandomKey(8);
        var key = RandomData.GenerateRandomKey(32);
        var passwordHasher = new PasswordHasher<string>();
        var hashedKey = passwordHasher.HashPassword(keyName, key);
        
        
        return new NewServerKeyRegister(keyName,key);
    }
}
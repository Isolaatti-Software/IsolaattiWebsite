using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.EmailSender;
using Isolaatti.Enums;
using Isolaatti.Repositories;
using Microsoft.AspNetCore.Http;

namespace Isolaatti.Accounts.Service;

public class ProfileService(DbContextApp db)
{
    
    private const string AllowedCharactersForUsername = "ABCDEFGHIJKLMNOPKRSTUVWXYZabcdefghijklmnopkrstuvwxyz1234567890_-";
    
    private static bool IsUsernameValid(string username)
    {
        return username.All(character => AllowedCharactersForUsername.Contains(character));
    }
    
    private static bool IsDisplayNameValid(string displayName)
    {
        return displayName.Length > 1;
    }

    public async Task<Profile> CreateProfile(string userId, string displayName, string email, string uniqueUsername)
    {
        var profile = new Profile()
        {
            SID = userId,
            Email = email,
            Name = displayName,
            UniqueUsername = uniqueUsername
        };

        db.Users.Add(profile);
        await db.SaveChangesAsync();

        return profile;
    }
    
}
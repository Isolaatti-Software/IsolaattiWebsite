using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.DTOs;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Isolaatti.Utils;

namespace Isolaatti.Accounts.Service;

public interface IAccountsService
{
    public enum AccountPrecreateResult
    {
        EmailUsed,
        Success,
        EmailValidationError
    }
    
    Task<AccountMakingResult> MakeAccountAsync(string username, string displayName, string email, string password);
    Task<AccountPrecreateResult> PreCreateAccount(string email);
    Task<bool> IsUserEmailVerified(int userId);
    Task<bool> ChangeAPassword(int userId, string currentPassword, string newPassword);
    Task<string> CreateNewSession(int userId, string plainTextPassword);
    Task<User> ValidateSession(SessionDto sessionDto);

    Task<bool> RemoveSession(SessionDto sessionDto);
    string GetUsernameFromId(int userId);
    IEnumerable<Session> GetSessionsOfUser(int userId);
    string GetIpAddress();
    void RemoveSessionCookie();
    Task<Session> CurrentSession();
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.DTOs;
using Isolaatti.Models.MongoDB;

namespace Isolaatti.Accounts.Service;

public interface IAccountsService
{
    public enum AccountPrecreateResult
    {
        EmailUsed,
        Success,
        EmailValidationError,
        CodesSentLimitReached
    }
    
    
    Task<AccountMakeResult> MakeAccountAsync(string username, string displayName, string email, string password);
    Task<AccountPrecreateResult> PreCreateAccount(string email);
    
    /// <summary>
    /// Validates code. Expiration is validated. Code is marked as valid if it is valid.
    /// </summary>
    /// <param name="code"></param>
    /// <returns>The AccountPrecreate or null when code is invalid or does not exist</returns>
    Task<AccountPrecreate?> ValidatePreCreateCode(string code);
    Task<ChangePasswordResultDto> ChangeAPassword(int userId, string currentPassword, string newPassword);
    Task<string?> CreateNewSession(int userId, string plainTextPassword);
    Task<User?> ValidateSession(SessionDto sessionDto);

    Task<bool> RemoveSession(SessionDto sessionDto);
    Task<bool> RemoveSessions(int userId, IEnumerable<string> ids);
    Task<bool> RemoveAllSessions(int userId, IEnumerable<string>? exceptIds);
    string GetUsernameFromId(int userId);
    Task<IEnumerable<Session>> GetSessionsOfUser(int userId);
    string GetIpAddress();
    void RemoveSessionCookie();
    Task<Session> CurrentSession();
}
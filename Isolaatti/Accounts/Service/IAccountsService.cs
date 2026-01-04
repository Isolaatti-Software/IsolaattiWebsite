using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Accounts.SignIn.Data;

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
    
    
    Task<AccountMakeResult> MakeAccountAsync(string username, string firstName, string lastName, string email,
        string password);
    Task<AccountPrecreateResult> PreCreateAccount(string email);
    
    /// <summary>
    /// Validates code. Expiration is validated. Code is marked as valid if it is valid.
    /// </summary>
    /// <param name="code"></param>
    /// <returns>The AccountPrecreate or null when code is invalid or does not exist</returns>
    Task<AccountPrecreate?> ValidatePreCreateCode(string code);
    
    string? GetUsernameFromId(int userId);
    string GetIpAddress();
    
    Task<User?> GetUserFromSub(Claim sub);
}
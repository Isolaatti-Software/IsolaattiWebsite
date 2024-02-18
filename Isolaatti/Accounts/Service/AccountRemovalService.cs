using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Config;
using Isolaatti.EmailSender;
using Isolaatti.isolaatti_lib;
using Isolaatti.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Isolaatti.Accounts.Service;

public class AccountRemovalService
{
    private readonly IOptions<HostConfig> _optionsHostConfig;
    private readonly DbContextApp _db;
    private readonly EmailSenderMessaging _emailSender;

    public AccountRemovalService(DbContextApp db, IOptions<HostConfig> optionsHostConfig, EmailSenderMessaging emailSender)
    {
        _db = db;
        _optionsHostConfig = optionsHostConfig;
        _emailSender = emailSender;
    }

    public async Task SendEmail(string email)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email.Equals(email));
        if (user == null)
        {
            return;
        }

        var key = Utils.RandomData.GenerateRandomKey(15);

        var passwordHasher = new PasswordHasher<string>();
        
        var keyHash = passwordHasher.HashPassword(user.Id.ToString(), key);

        var accountRemovalRequestEntity = new AccountRemovalRequestEntity()
        {
            UserId = user.Id,
            KeyHash = keyHash
        };

        _db.AccountRemovalRequests.Add(accountRemovalRequestEntity);
        await _db.SaveChangesAsync();
        
        // enqueue email
        var url = 
            $"https://{_optionsHostConfig.Value.BaseUrl}/AccountRemoval/ConfirmAccountRemoval?id={accountRemovalRequestEntity.Id}&key={HttpUtility.UrlEncode(key)}";
        var body = string.Format(EmailTemplates.DeleteAccountEmail, url);
        _emailSender.SendEmail(
            "no-reply@isolaatti.com", 
            "Isolaatti", 
            user.Email, 
            user.Name, 
            "Elimina tu cuenta de Isolaatti", 
            body);
    }

    public async Task<bool> ProceedWithAccountRemoval(Guid accountRemovalRequestId, string key)
    {
        var accountRemovalRequest = await _db.AccountRemovalRequests.FindAsync(accountRemovalRequestId);
        if (accountRemovalRequest == null)
        {
            return false;
        }
        
        // check key

        var hasher = new PasswordHasher<string>();

        var hashVerificationResult = hasher.VerifyHashedPassword(accountRemovalRequest.UserId.ToString(), accountRemovalRequest.KeyHash, key);

        if (hashVerificationResult == PasswordVerificationResult.Failed)
        {
            return false;
        }

        await DeleteAudios(accountRemovalRequest.UserId);
        await DeleteImages(accountRemovalRequest.UserId);
        await DeleteSessions(accountRemovalRequest.UserId);
        await HandleUserRemovalFromSquads(accountRemovalRequest.UserId);
        await DeleteAccount(accountRemovalRequest.UserId);
        
        return true;
    }
    
    // remove data that is not in relational database first
    private async Task DeleteImages(int userId)
    {
        
    }

    private async Task DeleteSessions(int userId)
    {
        
    }

    private async Task DeleteAudios(int userId)
    {
        
    }
    
    // special case for squads:
    // check every squad user owns.
    // 1. if squad will have 0 members remaining, squad will be removed entirely
    // 2. if it has some admin, whoever has more "points" will be promoted to owner
    // 3. if no admin, ordinary member with the most "points" will be promoted to owner
    private async Task HandleUserRemovalFromSquads(int userId)
    {
        
    }
    
    // deleting user will cause all related data to be removed in cascade
    private async Task DeleteAccount(int userId)
    {
        
    }
}
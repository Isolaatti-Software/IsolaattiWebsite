using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Accounts.SignIn.Data;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.EmailSender;
using Isolaatti.Enums;
using Isolaatti.IdentityService;
using Isolaatti.Repositories;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.RulesetToEditorconfig;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Accounts.Service;

public class AccountsService : IAccountsService
{
    private readonly DbContextApp db;
    private readonly HttpContext _httpContext;
    private readonly EmailSenderMessaging _emailSender;
    private readonly SessionsRepository _sessionsRepository;
    private readonly IIdentityService _identityService;

    public const string SessionCookieName = "isolaatti_user_session_token";
    private const string FromAddress = "cuentas@isolaatti.com";
    private const string FromName = "Isolaatti cuentas";
    private const string WelcomeSubject = "Te damos la bienvenida a Isolaatti";
    private const string JustLoggedInSubject = "Iniciaste sesión en Isolaatti";
    private const string PrecreateAccountSubject = "Continuemos con la creación de tu cuenta en Isolaatti";
    
    private const string AllowedCharactersForUsername = "ABCDEFGHIJKLMNOPKRSTUVWXYZabcdefghijklmnopkrstuvwxyz1234567890_-";



    public AccountsService(DbContextApp db,
        EmailSenderMessaging emailSender,
        ScopedHttpContext scopedHttpContext,
        SessionsRepository sessionsRepository,
        IIdentityService identityService)
    {
        this.db = db;
        _emailSender = emailSender;
        _httpContext = scopedHttpContext.HttpContext;
        _sessionsRepository = sessionsRepository;
        _identityService = identityService;
    }

    private static bool IsUsernameValid(string username)
    {
        return username.All(character => AllowedCharactersForUsername.Contains(character));
    }
    

    private static bool IsPasswordValid(string password)
    {
        return password.Length >= 6;
    }

    private static bool IsDisplayNameValid(string displayName)
    {
        return displayName.Length > 1;
    }

    public async Task<User?> GetUserFromSub(Claim sub)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Sub == sub.Value);
    }

    public async Task<AccountMakeResult> MakeAccountAsync(string username, string firstName, string lastName, string email,
        string password)
    {
        
        try
        {
            if (await _identityService.UserExists(email))
            {
                return new AccountMakeResult { AccountMakingResult = AccountMakingResult.EmailNotAvailable };
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        var newUser = new User
        {
            UniqueUsername = username,
            Name = firstName,
            LastName = lastName
        };

        try
        {
            var userId = await _identityService.CreateUser(username, email, password, firstName, lastName);
            db.AccountPrecreates.RemoveRange(db.AccountPrecreates.Where(pc => pc.Email.Equals(email)));
            await db.SaveChangesAsync();
            newUser.Sub = userId;
            db.Users.Add(newUser);
            await db.SaveChangesAsync();
            SendWelcomeEmail(email, newUser.Name);
            return new AccountMakeResult { AccountMakingResult = AccountMakingResult.Ok, UserId = newUser.Id };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new AccountMakeResult { AccountMakingResult = AccountMakingResult.Error };
        }
    }
    

    public async Task<IAccountsService.AccountPrecreateResult> PreCreateAccount(string email)
    {

        if (!new EmailAddressAttribute().IsValid(email))
        {
            return IAccountsService.AccountPrecreateResult.EmailValidationError;
        }
        
        if (await _identityService.UserExists(email))
        {
            return IAccountsService.AccountPrecreateResult.EmailUsed;
        }

        if (await db.AccountPrecreates.CountAsync(ap => ap.Email.Equals(email)) > 4)
        {
            return IAccountsService.AccountPrecreateResult.CodesSentLimitReached;
        }
        
        var precreate = new AccountPrecreate()
        {
            Id = RandomData.GenerateRandomString(6),
            Email = email
        };

        await db.AccountPrecreates.AddAsync(precreate);

        await db.SaveChangesAsync();
        
        _emailSender.SendEmail(FromAddress, FromName, email, string.Empty, PrecreateAccountSubject, string.Format(EmailTemplates.PreRegistrationEmail.Trim(), precreate.Id));

        return IAccountsService.AccountPrecreateResult.Success;
    }

    public async Task<AccountPrecreate?> ValidatePreCreateCode(string code)
    {
        var foundCode = await db.AccountPrecreates.FindAsync(code);
        if (foundCode == null || await _identityService.UserExists(foundCode.Email))
        {
            return null;
        }

        if (foundCode.Validated)
        {
            return foundCode;
        }

        var isValid = foundCode.CreationDateTime.AddMinutes(10).CompareTo(DateTime.UtcNow) >= 0;

        if (!isValid) return null;
        
        // Code is valid, mark this as validated so that future validation returns true even if code expired
        // Use case for this is to allow account creation form to continue working after verifying code is valid.
        foundCode.Validated = true;
        db.AccountPrecreates.Update(foundCode);
        await db.SaveChangesAsync();

        return foundCode;
    }

    public string? GetUsernameFromId(int userId)
    {
        return db.Users.Where(u => u.Id == userId).Select(u => u.Name).FirstOrDefault();
    }

    private void SendWelcomeEmail(string email, string name)
    {
        var htmlBody = string.Format(EmailTemplates.WelcomeEmail, name);
        _emailSender.SendEmail(FromAddress,FromName,email, name, WelcomeSubject, htmlBody);
    }
    

    public string GetIpAddress()
    {
        var xForwardedForHeaderValue = _httpContext.Request.Headers["X-Forwarded-For"];
        return xForwardedForHeaderValue.Count == 0 
                ? _httpContext.Request.HttpContext.Connection.RemoteIpAddress?.ToString() 
                : _httpContext.Request.Headers["X-Forwarded-For"][0].Split(",")[0].Trim();
    }
    
    
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.DTOs;
using Isolaatti.EmailSender;
using Isolaatti.Enums;
using Isolaatti.isolaatti_lib;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Accounts.Service;

public partial class AccountsService : IAccountsService
{
    private readonly DbContextApp db;
    private readonly HttpContext _httpContext;
    private readonly EmailSenderMessaging _emailSender;
    private readonly SessionsRepository _sessionsRepository;

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
        SessionsRepository sessionsRepository)
    {
        this.db = db;
        _emailSender = emailSender;
        _httpContext = scopedHttpContext.HttpContext;
        _sessionsRepository = sessionsRepository;
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
    
    
    
    public async Task<AccountMakingResult> MakeAccountAsync(string username, string displayName, string email,
        string password)
    {
        
        if (!IsPasswordValid(password) || !IsDisplayNameValid(displayName) || !IsUsernameValid(username))
        {
            return AccountMakingResult.ValidationProblems;
        }
        
        if (await db.Users.AnyAsync(user => user.Email.Equals(email)))
        {
            return AccountMakingResult.EmailNotAvailable;
        }

        var passwordHasher = new PasswordHasher<string>();
        var hashedPassword = "";
        await Task.Run(() => { hashedPassword = passwordHasher.HashPassword(email, password); });
        var newUser = new User
        {
            UniqueUsername = username,
            Name = displayName,
            Email = email,
            Password = hashedPassword,
            EmailValidated = true
        };
        try
        {
            db.Users.Add(newUser);
            await db.SaveChangesAsync();
            SendWelcomeEmail(newUser.Email, newUser.Name);
            return AccountMakingResult.Ok;
        }
        catch (Exception)
        {
            return AccountMakingResult.Error;
        }
    }
    

    public async Task<IAccountsService.AccountPrecreateResult> PreCreateAccount(string email)
    {

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            return IAccountsService.AccountPrecreateResult.EmailUsed;
        }
        
        var precreate = new AccountPrecreate()
        {
            Id = RandomData.GenerateRandomString(6),
            Email = email
        };

        await db.AccountPrecreates.AddAsync(precreate);

        await db.SaveChangesAsync();

        if (!new EmailAddressAttribute().IsValid(email))
        {
            return IAccountsService.AccountPrecreateResult.EmailValidationError;
        }
        
        
        _emailSender.SendEmail(FromAddress, FromName, email, string.Empty, PrecreateAccountSubject, string.Format(EmailTemplates.PreRegistrationEmail.Trim(), precreate.Id));

        return IAccountsService.AccountPrecreateResult.Success;
    }

    public async Task<bool> IsUserEmailVerified(int userId)
    {
        var user = await db.Users.FindAsync(userId);
        return user.EmailValidated;
    }

    public async Task<bool> ChangeAPassword(int userId, string currentPassword, string newPassword)
    {
        var user = await db.Users.FindAsync(userId);
        if (user == null) return false;
        var passwordHasher = new PasswordHasher<string>();
        var verificationResult =
            passwordHasher.VerifyHashedPassword(user.Email, user.Password, currentPassword);
        if (verificationResult == PasswordVerificationResult.Failed) return false;

        var newPasswordHashed = passwordHasher.HashPassword(user.Email, newPassword);
        user.Password = newPasswordHashed;
        db.Users.Update(user);
        await db.SaveChangesAsync();
        return true;
    }


    public async Task<string> CreateNewSession(int userId, string plainTextPassword)
    {
        var user = await db.Users.FindAsync(userId);
        if (user == null) return null;
        var passwordHasher = new PasswordHasher<string>();
        var passwordVerificationResult =
            passwordHasher.VerifyHashedPassword(user.Email, user.Password, plainTextPassword);
        if (passwordVerificationResult == PasswordVerificationResult.Failed) return null;

        var sessionInserted = await _sessionsRepository.InsertSession(new Session()
        {
            UserId = user.Id,
            IpAddress = GetIpAddress(),
            UserAgent = GetUserAgent()
        });
        SendJustLoginEmail(user.Email, user.Name,GetIpAddress());

        return sessionInserted.ToString();
    }

    public async Task<User> ValidateSession(SessionDto sessionDto)
    {
        if (sessionDto == null) return null;   
        var userId = await _sessionsRepository.FindUserIdFromSession(sessionDto);
        var user = await db.Users.FindAsync(userId);
        return user;
    }
    
    public async Task<bool> RemoveSession(SessionDto sessionDto)
    {
        return await _sessionsRepository.RemoveSession(sessionDto);
    }
    

    public string GetUsernameFromId(int userId)
    {
        return db.Users.Where(u => u.Id == userId).Select(u => u.Name).FirstOrDefault();
    }

    private void SendWelcomeEmail(string email, string name)
    {
        var htmlBody = string.Format(EmailTemplates.WelcomeEmail, name);
        _emailSender.SendEmail(FromAddress,FromName,email, name, WelcomeSubject, htmlBody);
    }

    private void SendWelcomeEmailForExternal(string email, string name, string generatedPassword)
    {
        
        var htmlBody = string.Format(EmailTemplates.WelcomeEmailExternal, name, email, generatedPassword);
        _emailSender.SendEmail(FromAddress,FromName,email, name, WelcomeSubject, htmlBody);
    }

    public void SendJustLoginEmail(string email, string name, string ipAddress)
    {
        var htmlBody = string.Format(EmailTemplates.LoginEmail, name, ipAddress);
        _emailSender.SendEmail(FromAddress, FromName, email, name, JustLoggedInSubject, htmlBody);
    }

    public IEnumerable<Session> GetSessionsOfUser(int userId)
    {
        return _sessionsRepository.FindSessionsOfUser(userId);
    }

    public string GetIpAddress()
    {
        var xForwardedForHeaderValue = _httpContext.Request.Headers["X-Forwarded-For"];
        return xForwardedForHeaderValue.Count == 0 
                ? _httpContext.Request.HttpContext.Connection.RemoteIpAddress?.ToString() 
                : _httpContext.Request.Headers["X-Forwarded-For"][0].Split(",")[0].Trim();
    }

    public string GetUserAgent()
    {
        return _httpContext.Request.Headers.UserAgent.ToString();
    }

    public void RemoveSessionCookie()
    {
        _httpContext.Response.Cookies.Delete(SessionCookieName);
    }

    public async Task<Session> CurrentSession()
    {
        var cookie = _httpContext.Request.Cookies[SessionCookieName];
        if (cookie == null)
        {
            return null;
        }

        try
        {
            var sessionDto = JsonSerializer.Deserialize<SessionDto>(cookie);
            return await _sessionsRepository.FindSessionById(sessionDto);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
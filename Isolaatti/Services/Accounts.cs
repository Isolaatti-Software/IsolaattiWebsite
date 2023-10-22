using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using Isolaatti.DTOs;
using Isolaatti.EmailSender;
using Isolaatti.Enums;
using Isolaatti.isolaatti_lib;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Isolaatti.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Services;

public class Accounts : IAccounts
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

    public Accounts(DbContextApp db,
        EmailSenderMessaging emailSender,
        ScopedHttpContext scopedHttpContext,
        SessionsRepository sessionsRepository)
    {
        this.db = db;
        _emailSender = emailSender;
        _httpContext = scopedHttpContext.HttpContext;
        _sessionsRepository = sessionsRepository;
    }

    public async Task<AccountMakingResult> MakeAccountAsync(string username, string displayName, string email,
        string password)
    {
        


        if (await db.Users.AnyAsync(user => user.Email.Equals(email)))
        {
            return AccountMakingResult.EmailNotAvailable;
        }

        if (username == "" || password == "" || email == "" || displayName == "")
        {
            return AccountMakingResult.EmptyFields;
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
    

    public async Task MakeAccountFromGoogleAccount(string accessToken)
    {
        var decodedToken   = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(accessToken);
        
        var uid = decodedToken.Uid;
        var user = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
        

        if (!db.Users.Any(u => u.Email.Equals(user.Email)))
        {
            var randomPassword = Utils.RandomData.GenerateRandomPassword();
            await MakeAccountAsync(user.DisplayName, user.DisplayName + Utils.RandomData.GenerateRandomKey(6), user.Email, randomPassword);
            SendWelcomeEmailForExternal(user.Email, user.DisplayName, randomPassword);
        }

        var isolaattiUser = db.Users.Single(u => u.Email.Equals(user.Email));

        if (await db.ExternalUsers.AnyAsync(googleUser =>
                googleUser.GoogleUid.Equals(user.Uid) && googleUser.UserId.Equals(isolaattiUser.Id))) return;

        // Add relation between Isolaatti account and Google Account
        var googleUser = new ExternalUser
        {
            UserId = isolaattiUser.Id,
            GoogleUid = user.Uid
        };

        var googleProfileImageUrl = user.PhotoUrl;
        if (googleProfileImageUrl != null)
        {
            isolaattiUser.ProfileImageUrl = googleProfileImageUrl;
            db.Users.Update(isolaattiUser);
        }
        db.ExternalUsers.Add(googleUser);
        await db.SaveChangesAsync();
    }

    public async Task<string> CreateTokenForGoogleUser(string accessToken)
    {
        var decodedTokenTask = FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(accessToken);
        var uid = (await decodedTokenTask).Uid;
        var relation = db.ExternalUsers.Single(u => u.GoogleUid.Equals(uid));
        var user = await db.Users.FindAsync(relation.UserId);

        if (user == null) return null;
        
        var insertedSession = await _sessionsRepository.InsertSession(new Session()
        {
            UserId = user.Id,
            IpAddress = GetIpAddress(),
            UserAgent = GetUserAgent()
        });
        SendJustLoginEmail(user.Email, user.Name,GetIpAddress());

        return insertedSession.ToString();
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
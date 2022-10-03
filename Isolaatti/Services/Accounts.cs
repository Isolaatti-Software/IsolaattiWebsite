using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using Isolaatti.Classes.Authentication;
using Isolaatti.Enums;
using Isolaatti.isolaatti_lib;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Isolaatti.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Isolaatti.Services;

public class Accounts : IAccounts
{
    private readonly DbContextApp db;
    private readonly HttpContext _httpContext;
    private readonly ISendGridClient _sendGridClient;
    private readonly AuthTokensRepository _authTokensRepository;
    private readonly KeyGenService _keyGenService;

    public Accounts(DbContextApp db,
        ISendGridClient sendGridClient,
        ScopedHttpContext scopedHttpContext,
        AuthTokensRepository authTokensRepository,
        KeyGenService keyGenService)
    {
        this.db = db;
        _sendGridClient = sendGridClient;
        _httpContext = scopedHttpContext.HttpContext;
        _authTokensRepository = authTokensRepository;
        _keyGenService = keyGenService;
    }

    public async Task<AccountMakingResult> MakeAccountAsync(string username, string email, string password)
    {
        // Now I don't care about usernames availability


        if (await db.Users.AnyAsync(user => user.Email.Equals(email)))
        {
            return AccountMakingResult.EmailNotAvailable;
        }

        if (username == "" || password == "" || email == "")
        {
            return AccountMakingResult.EmptyFields;
        }

        var passwordHasher = new PasswordHasher<string>();
        var hashedPassword = "";
        await Task.Run(() => { hashedPassword = passwordHasher.HashPassword(email, password); });
        var newUser = new User
        {
            Name = username,
            Email = email,
            Password = hashedPassword,
            EmailValidated = true
        };
        try
        {
            db.Users.Add(newUser);
            await db.SaveChangesAsync();
            await SendWelcomeEmail(newUser.Email, newUser.Name);
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


    public async Task<AuthenticationTokenSerializable> CreateNewToken(int userId, string plainTextPassword)
    {
        var user = await db.Users.FindAsync(userId);
        if (user == null) return null;
        var passwordHasher = new PasswordHasher<string>();
        var passwordVerificationResult =
            passwordHasher.VerifyHashedPassword(user.Email, user.Password, plainTextPassword);
        if (passwordVerificationResult == PasswordVerificationResult.Failed) return null;

        var token = _keyGenService.GenerateKeyAndHash();


        // This object is stored. It includes only the hashed key
        var authToken = new AuthToken()
        {
            Guid = token.Item1,
            HashedKey = passwordHasher.HashPassword(token.Item1, token.Item2),
            UserAgent = _httpContext.Request.Headers.UserAgent,
            IpAddress = _httpContext.Connection.RemoteIpAddress?.ToString(),
            CreationDate = DateTime.Now.ToUniversalTime(),
            UserId = userId
        };

        var result = await _authTokensRepository.StoreToken(authToken);

        // This object is given to the user. Call ToString()
        var tokenSerializable = new AuthenticationTokenSerializable()
        {
            Id = result.Id,
            Guid = result.Guid,
            Secret = token.Item2
        };

        return tokenSerializable;
    }

    public async Task<User> ValidateToken(string token)
    {
        if (token == null) return null;
        var serializableToken = AuthenticationTokenSerializable.FromString(token);
        var foundToken = await _authTokensRepository.FindToken(serializableToken.Id);
        if (foundToken == null)
        {
            return null;
        }

        // Secret validation
        var passwordHasher = new PasswordHasher<string>();
        var validationResult = passwordHasher.VerifyHashedPassword(serializableToken.Guid.ToString(),
            foundToken.HashedKey, serializableToken.Secret);
        if (validationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var user = await db.Users.FindAsync(foundToken.UserId);
        return user;
    }

    public async Task RemoveAToken(int userId, string id)
    {
        var foundToken = await _authTokensRepository.FindToken(id);
        if (foundToken == null)
        {
            return;
        }

        if (foundToken.UserId != userId)
        {
            return;
        }

        await _authTokensRepository.RemoveToken(foundToken.Id);
    }

    public async Task RemoveAllUsersTokens(int userId)
    {
        await _authTokensRepository.RemoveTokenOfUser(userId);
    }

    public async Task MakeAccountFromGoogleAccount(string accessToken)
    {
        var decodedTokenTask = FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(accessToken);

        var uid = (await decodedTokenTask).Uid;
        var userTask = FirebaseAuth.DefaultInstance.GetUserAsync(uid);

        var user = await userTask;

        if (!db.Users.Any(u => u.Email.Equals(user.Email)))
        {
            var randomPassword = Utils.RandomData.GenerateRandomPassword();
            await MakeAccountAsync(user.DisplayName, user.Email, randomPassword);
            await SendWelcomeEmailForExternal(user.Email, user.DisplayName, randomPassword);
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
        db.ExternalUsers.Add(googleUser);
        await db.SaveChangesAsync();
    }

    public async Task<AuthenticationTokenSerializable> CreateTokenForGoogleUser(string accessToken)
    {
        var decodedTokenTask = FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(accessToken);
        var uid = (await decodedTokenTask).Uid;
        var relation = db.ExternalUsers.Single(u => u.GoogleUid.Equals(uid));
        var user = await db.Users.FindAsync(relation.UserId);

        if (user == null) return null;

        var passwordHasher = new PasswordHasher<string>();

        var token = _keyGenService.GenerateKeyAndHash();

        // This object is stored. It includes only the hashed key
        var authToken = new AuthToken()
        {
            Guid = token.Item1,
            HashedKey = passwordHasher.HashPassword(token.Item1, token.Item2),
            UserAgent = _httpContext.Request.Headers.UserAgent,
            IpAddress = _httpContext.Connection.RemoteIpAddress?.ToString(),
            CreationDate = DateTime.Now.ToUniversalTime(),
            UserId = user.Id
        };

        var result = await _authTokensRepository.StoreToken(authToken);

        // This object is given to the user. Call ToString()
        var tokenSerializable = new AuthenticationTokenSerializable()
        {
            Id = result.Id,
            Guid = result.Guid,
            Secret = token.Item2
        };

        return tokenSerializable;
    }

    public string GetUsernameFromId(int userId)
    {
        return db.Users.Where(u => u.Id == userId).Select(u => u.Name).FirstOrDefault();
    }

    private async Task SendWelcomeEmail(string email, string name)
    {
        var from = new EmailAddress("cuentas@isolaatti.com", "Isolaatti");
        var to = new EmailAddress(email, name);
        var subject = "Te damos la bienvenida a Isolaatti";
        var htmlBody = MailHelper.CreateSingleEmail(from, to, subject,
            "Bienvenid@ a Isolaatti",
            string.Format(EmailTemplates.WelcomeEmail, name));
        await _sendGridClient.SendEmailAsync(htmlBody);
    }

    private async Task SendWelcomeEmailForExternal(string email, string name, string generatedPassword)
    {
        var from = new EmailAddress("cuentas@isolaatti.com", "Isolaatti");
        var to = new EmailAddress(email, name);
        var subject = "Te damos la bienvenida a Isolaatti";
        var htmlBody = MailHelper.CreateSingleEmail(from, to, subject,
            "Bienvenid@ a Isolaatti",
            string.Format(EmailTemplates.WelcomeEmailExternal, name, email, generatedPassword));
        await _sendGridClient.SendEmailAsync(htmlBody);
    }

    public async Task SendJustLoginEmail(string email, string name, string ipAddress)
    {
        var from = new EmailAddress("cuentas@isolaatti.com", "Isolaatti");
        var to = new EmailAddress(email, name);
        var subject = "Iniciaste sesión en Isolaatti";
        var htmlBody = MailHelper.CreateSingleEmail(from, to, subject,
            "Iniciaste sesión en Isolaatti...",
            string.Format(EmailTemplates.LoginEmail, name, ipAddress));
        await _sendGridClient.SendEmailAsync(htmlBody);
    }

    public IEnumerable<AuthToken> GetTokenOfUser(int userId)
    {
        return _authTokensRepository.FindTokenOfUser(userId);
    }
}
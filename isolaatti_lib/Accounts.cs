/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

//Make objects of this class when tasks related to accounts will be performed.
//These methods are normally used by controllers (for API) and Razor pages

using System;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using isolaatti_API.Enums;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace isolaatti_API.isolaatti_lib
{
    public class Accounts
    {
        private readonly DbContextApp db;
        private HttpRequest _request;

        public Accounts(DbContextApp db)
        {
            this.db = db;
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
                Uid = Guid.NewGuid().ToString(),
                UserPreferencesJson = "{}",
                FollowersIdsJson = "[]",
                FollowingIdsJson = "[]",
                EmailValidated = true
            };
            try
            {
                db.Users.Add(newUser);
                await db.SaveChangesAsync();
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

        public void DefineHttpRequestObject(HttpRequest request)
        {
            _request = request;
        }

        public async Task<SessionToken> CreateNewToken(int userId, string plainTextPassword)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) return null;
            var passwordHasher = new PasswordHasher<string>();
            var passwordVerificationResult =
                passwordHasher.VerifyHashedPassword(user.Email, user.Password, plainTextPassword);
            if (passwordVerificationResult == PasswordVerificationResult.Failed) return null;

            var tokenObj = new SessionToken()
            {
                UserId = user.Id,
                IpAddress = _request.HttpContext.Connection.RemoteIpAddress.ToString(),
                UserAgent = _request.Headers
                    .ContainsKey(HeaderNames.UserAgent)
                    ? _request.Headers[HeaderNames.UserAgent].ToString()
                    : "Not provided"
            };
            db.SessionTokens.Add(tokenObj);
            await db.SaveChangesAsync();
            return tokenObj;
        }

        public async Task<User> ValidateToken(string token)
        {
            try
            {
                var tokenObj = await db.SessionTokens.SingleAsync(sessionToken => sessionToken.Token.Equals(token));
                var user = await db.Users.FindAsync(tokenObj.UserId);
                return user;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public async Task RemoveAToken(string token)
        {
            try
            {
                var tokenObj = await db.SessionTokens.SingleAsync(sessionToken => sessionToken.Token.Equals(token));
                db.SessionTokens.Remove(tokenObj);
                await db.SaveChangesAsync();
            }
            catch (InvalidOperationException)
            {
            }
        }

        public async Task RemoveAllUsersTokens(int userId)
        {
            var tokenObjs = db.SessionTokens.Where(sessionToken => sessionToken.UserId == userId);
            db.SessionTokens.RemoveRange(tokenObjs);
            await db.SaveChangesAsync();
        }

        public async Task MakeAccountFromGoogleAccount(string accessToken)
        {
            var decodedTokenTask = FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(accessToken);

            var uid = (await decodedTokenTask).Uid;
            var userTask = FirebaseAuth.DefaultInstance.GetUserAsync(uid);

            var user = await userTask;

            if (!db.Users.Any(u => u.Email.Equals(user.Email)))
            {
                await MakeAccountAsync(user.DisplayName, user.Email, GenerateRandomAlphaNumericPassword(10));
            }

            var isolaattiUser = db.Users.Single(u => u.Email.Equals(user.Email));

            if (await db.GoogleUsers.AnyAsync(googleUser =>
                    googleUser.GoogleUid.Equals(user.Uid) && googleUser.UserId.Equals(isolaattiUser.Id))) return;

            // Add relation between Isolaatti account and Google Account
            var googleUser = new GoogleUser
            {
                UserId = isolaattiUser.Id,
                GoogleUid = user.Uid
            };
            db.GoogleUsers.Add(googleUser);
            await db.SaveChangesAsync();
        }

        public async Task<SessionToken> CreateTokenForGoogleUser(string accessToken)
        {
            var decodedTokenTask = FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(accessToken);
            var uid = (await decodedTokenTask).Uid;
            var relation = db.GoogleUsers.Single(u => u.GoogleUid.Equals(uid));
            var user = await db.Users.FindAsync(relation.UserId);
            var sessionToken = new SessionToken
            {
                UserId = user.Id,
                IpAddress = _request.HttpContext.Connection.RemoteIpAddress.ToString(),
                UserAgent = _request.Headers
                    .ContainsKey(HeaderNames.UserAgent)
                    ? _request.Headers[HeaderNames.UserAgent].ToString()
                    : "Not provided"
            };

            return sessionToken;
        }

        private static string GenerateRandomAlphaNumericPassword(int lenght)
        {
            var password = "";
            do
            {
                password += Guid.NewGuid()
                    .ToString().Replace("-", string.Empty);
            } while (password.Length < lenght);

            return password.Remove(0, lenght - 1);
        }

        public static async Task SendWelcomeEmail(ISendGridClient sendGridClient, string email, string name)
        {
            var from = new EmailAddress("no-reply@isolaatti.com", "Isolaatti");
            var to = new EmailAddress(email, name);
            var subject = "Cambia tu contraseña de Isolaatti";
            var htmlBody = MailHelper.CreateSingleEmail(from, to, subject,
                "Bienvenid@ a Isolaatti",
                string.Format(EmailTemplates.PasswordRecoveryEmail, name));
            await sendGridClient.SendEmailAsync(htmlBody);
        }
    }
}
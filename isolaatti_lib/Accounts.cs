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
using FirebaseAdmin.Auth;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Net.Http.Headers;

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

        public string MakeAccount(string username, string email, string password)
        {
            if (db.Users.Any(user => user.Email.Equals(email)))
            {
                return "1";
            }

            if (db.Users.Any(user => user.Name.Equals(username)))
            {
                return "2";
            }

            if (username == "" || password == "" || email == "")
            {
                return "3";
            }

            var passwordHasher = new PasswordHasher<string>();
            User newUser = new User()
            {
                Name = username,
                Email = email,
                Password = passwordHasher.HashPassword(email, password),
                Uid = Guid.NewGuid().ToString(),
                UserPreferencesJson = "{}",
                FollowersIdsJson = "[]",
                FollowingIdsJson = "[]",
                EmailValidated = true
            };
            try
            {
                db.Users.Add(newUser);
                db.SaveChanges();
                //SendValidationEmail(newUser.Id, newUser.Uid);
                return "0";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public bool IsUserEmailVerified(int userId)
        {
            var user = db.Users.Find(userId);
            return user.EmailValidated;
        }

        /* Only to be used at admins portal (me)*/
        public bool DeleteAccount(int userId)
        {
            try
            {
                db.Users.Remove(db.Users.Find(userId));
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public bool ChangeAPassword(int userId, string currentPassword, string newPassword)
        {
            var user = db.Users.Find(userId);
            if (user == null) return false;
            var passwordHasher = new PasswordHasher<string>();
            var verificationResult =
                passwordHasher.VerifyHashedPassword(user.Email, user.Password, currentPassword);
            if (verificationResult == PasswordVerificationResult.Failed) return false;

            var newPasswordHashed = passwordHasher.HashPassword(user.Email, newPassword);
            user.Password = newPasswordHashed;
            db.Users.Update(user);
            db.SaveChanges();
            return true;
        }

        public void DefineHttpRequestObject(HttpRequest request)
        {
            _request = request;
        }

        public SessionToken CreateNewToken(int userId, string plainTextPassword)
        {
            var user = db.Users.Find(userId);
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
            db.SaveChanges();
            return tokenObj;
        }

        public User ValidateToken(string token)
        {
            try
            {
                var tokenObj = db.SessionTokens.Single(sessionToken => sessionToken.Token.Equals(token));
                var user = db.Users.Find(tokenObj.UserId);
                return user;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public void RemoveAToken(string token)
        {
            try
            {
                var tokenObj = db.SessionTokens.Single(sessionToken => sessionToken.Token.Equals(token));
                db.SessionTokens.Remove(tokenObj);
                db.SaveChanges();
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void RemoveAllUsersTokens(int userId)
        {
            var tokenObjs = db.SessionTokens.Where(sessionToken => sessionToken.UserId == userId);
            db.SessionTokens.RemoveRange(tokenObjs);
            db.SaveChanges();
        }

        public void MakeAccountFromGoogleAccount(string accessToken)
        {
            var decodedTokenTask = FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(accessToken);
            decodedTokenTask.Wait();

            var uid = decodedTokenTask.Result.Uid;
            var userTask = FirebaseAuth.DefaultInstance.GetUserAsync(uid);
            userTask.Wait();

            var user = userTask.Result;

            // makes the isolaatti account taking the google user information
            // Generates a random password. It is not needed for the user to know it, as the user will use google account
            // to sign in. User can still change the password and sign in in the normal way
            var state = MakeAccount(user.DisplayName, user.Email, GenerateRandomAlphaNumericPassword(10));

            // no errors when making account
            if (state.Equals("0"))
            {
                var isolaattiUser = db.Users.Single(u => u.Email.Equals(user.Email));
                // Add relation between Isolaatti account and Google Account
                var googleUser = new GoogleUser()
                {
                    UserId = isolaattiUser.Id,
                    GoogleUid = user.Uid
                };
                db.GoogleUsers.Add(googleUser);
                db.SaveChanges();
            }
        }

        public SessionToken CreateTokenForGoogleUser(string accessToken)
        {
            var decodedTokenTask = FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(accessToken);
            decodedTokenTask.Wait();
            var uid = decodedTokenTask.Result.Uid;
            var relation = db.GoogleUsers.Single(u => u.GoogleUid.Equals(uid));
            var user = db.Users.Find(relation.UserId);
            var sessionToken = new SessionToken()
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
    }
}
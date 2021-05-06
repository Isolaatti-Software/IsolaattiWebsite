using System;
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Identity;

namespace isolaatti_API.isolaatti_lib
{
    public class AdminAccounts
    {
        private readonly DbContextApp _db;

        public const string StatusNewPasswordVerificationFailed = "new_password_confirmation_failed";
        public const string StatusCurrentPasswordIsIncorrect = "provided_password_incorrect";
        public const string StatusPasswordChangedSuccess = "password_changed_successfully";
        public const string StatusEmailChangedSuccessfully = "email_changed_successfully";
        public const string StatusEmailIsNotValid = "email_invalid";

        public AdminAccounts(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        public bool MakeAccount(string username, string email, string password)
        {
            if (
                _db.AdminAccounts.Any(account => account.email.Equals(email)) ||
                _db.AdminAccounts.Any(account => account.name.Equals(username))
                )
            {
                return false;
            }
            var passwordHasher = new PasswordHasher<string>();
            var newAdminAccount = new AdminAccount()
            {
                email = email,
                name = username,
                password = passwordHasher.HashPassword(username, password)
            };

            _db.AdminAccounts.Add(newAdminAccount);
            _db.SaveChanges();
            
            return true;
        }

        public string CreateSessionToken(int id, string password)
        {
            var adminUser = _db.AdminAccounts.Find(id);
            if (adminUser == null) return null;
            var passwordHasher = new PasswordHasher<string>();
            var verificationResult = passwordHasher
                .VerifyHashedPassword(adminUser.name, adminUser.password, password);
            if (verificationResult != PasswordVerificationResult.Success) return null;
            var tokenObj = new AdminAccountSessionToken()
            {
                AccountId = adminUser.Id
            };
            _db.AdminAccountSessionTokens.Add(tokenObj);
            _db.SaveChanges();
            return tokenObj.Token;
        }

        public AdminAccount ValidateSessionToken(string token)
        {
            try
            {
                var tokenObj = _db.AdminAccountSessionTokens.Single(tokenS => tokenS.Token.Equals(token));
                var user = _db.AdminAccounts.Find(tokenObj.AccountId);
                return user;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public bool RemoveAccount(int id)
        {
            return true;
        }
    }
}
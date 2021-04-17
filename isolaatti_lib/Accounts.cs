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
using System.Net.Mail;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace isolaatti_API.isolaatti_lib
{
    public class Accounts
    {
        private readonly DbContextApp db;
        public Accounts(DbContextApp db)
        {
            this.db = db;
        }
        public string MakeAccount(string username, string email, string password)
        {
            if(db.Users.Any(user => user.Email.Equals(email)))
            {
                return "1";
            }
            if(db.Users.Any(user => user.Name.Equals(username)))
            {
                return "2";
            }
            if(username == "" || password == "" || email == "")
            {
                return "3";
            }
            User newUser = new User()
            {
                Name = username,
                Email = email,
                Password = password,
                Uid = Guid.NewGuid().ToString(),
                NotifyByEmail = true,
                NotifyWhenProcessFinishes = true,
                NotifyWhenProcessStarted = true,
                FollowersIdsJson = "[]",
                FollowingIdsJson = "[]"
            };
            try
            {
                db.Users.Add(newUser);
                db.SaveChanges();
                SendValidationEmail(newUser.Id, newUser.Uid);
                return "0";
            }
            catch(Exception e)
            {
                return e.ToString();
            }
        }

        public UserData LogIn(string email, string password)
        {
            if(!db.Users.Any(user => user.Email.Equals(email)))
            {
                return new UserData(false);
            }
            User userToEnter = db.Users.Single(user => user.Email.Equals(email));
            if(userToEnter.Password == password)
            {
                return new UserData(userToEnter.Id, db);
            }
            return new UserData(true); //bad password (see UserData class for more info)
        }
        
        /*This method is called internally when a new account was created successfully*/
        private bool SendValidationEmail(int id, string validationCode)
        {
            string link = $"http://isolaattiapi.azurewebsites.net/validateAccount?id={id}&code={validationCode}";
            string userEmailAddress = db.Users.Find(id).Email;
            string htmlBody = String.Format( @"
                <html>
                    <body>
                        <h1>Welcome to Isolaatti</h1>
                        <h1>Bienvenido a Isolaatti</h1>
                        <p>Recently, you used this email address to create a new account on Isolaatti. Click in the link below to activate your account</p>
                        <p>Recientemente, usaste esta dirección de correo electrónico para crear una nueva cuenta en Isolaatti. Haz clic en el link de abajo para activarla</p>
                        <a href='{0}'>{0}</a>
                        <p>If you didn't, please ommit this</p>
                        <p>Si no lo hiciste, omite esto por favor</p>
                    </body>
                </html>
                ",link);
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("Isolaatti","validation.isolaatti@gmail.com"));
            message.To.Add(new MailboxAddress(userEmailAddress));
            message.Subject = "Welcome to Isolaatti | Bienvenido a Isolaatti";
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlBody
            };
            using (var client = new SmtpClient())
            {
                try
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect("smtp.gmail.com", 465, true);
                    client.Authenticate("validation.isolaatti@gmail.com","0203_0302_" );
                    client.Send(message);
                    client.Disconnect(true);
                }
                catch (SmtpException exception)
                {
                    // this probably means the email address does not exist, so let's delete the account
                    var accountToDelete = db.Users.Find(id);
                    db.Users.Remove(accountToDelete);
                    db.SaveChanges();
                }
            }
            return true;
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
            try
            {
                var user = db.Users.Find(userId);
                if (user.Password.Equals(currentPassword))
                {
                    user.Password = newPassword;
                }
                else
                {
                    return false;
                }

                db.Users.Update(user);
                db.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public SessionToken CreateNewToken(int userId, string password)
        {
            var user = db.Users.Find(userId);
            if (user == null) return null;
            if (!user.Password.Equals(password)) return null;

            var tokenObj = new SessionToken()
            {
                UserId = user.Id
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
    }
}
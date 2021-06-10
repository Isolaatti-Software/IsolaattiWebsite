using System;
using System.Linq;
using System.Net.Mail;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace isolaatti_API.Pages
{
    public class ForgotPassword : PageModel
    {
        private readonly DbContextApp Db;
        public bool Post = false;
        public bool EmailFound;
        private User Account;

        public ForgotPassword(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        
        public void OnGet()
        {
            
        }

        public IActionResult OnPost([FromForm] string email)
        {
            Post = true;
            try
            {
                Account = Db.Users.Single(user => user.Email.Equals(email));
            }
            catch (InvalidOperationException)
            {
                EmailFound = false;
                return Page();
            }
            EmailFound = true;
            
            // here create token
            var userToken = new UserToken()
            {
                UserId = Account.Id,
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.Now.AddDays(1)
            };

            if (Db.UserTokens.Any(tok => tok.UserId.Equals(Account.Id)))
            {
                var oldToken = Db.UserTokens.Single(token => token.UserId.Equals(Account.Id));
                Db.UserTokens.Remove(oldToken);
            }

            Db.UserTokens.Add(userToken);
            Db.SaveChanges();
            
            // send email with link
            SendEmail(userToken.Token);

            return Page();
        }

        private void SendEmail(string token)
        {
            var link = $"https://{Request.HttpContext.Request.Host.Value}/RecoverPassword?token_s={token}";
            var userEmailAddress = Account.Email;
            var body = String.Format(EmailTemplates.PasswordRecoveryEmail, link);
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Isolaatti","validation.isolaatti@gmail.com"));
            message.To.Add(new MailboxAddress(userEmailAddress));
            message.Subject = "Isolaatti: Recover your password";
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };
            using var client = new SmtpClient();
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
                
            }
        }
    }
}
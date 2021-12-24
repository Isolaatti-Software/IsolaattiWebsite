using System;
using System.Linq;
using System.Threading.Tasks;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace isolaatti_API.Pages
{
    public class ForgotPassword : PageModel
    {
        private readonly DbContextApp Db;
        public bool Post = false;
        public bool EmailFound;
        private User Account;
        private ISendGridClient _sendGridClient;

        public ForgotPassword(DbContextApp dbContextApp, ISendGridClient sendGridClient)
        {
            Db = dbContextApp;
            _sendGridClient = sendGridClient;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPost([FromForm] string email)
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
            await Db.SaveChangesAsync();

            // send email with link
            await SendEmail(userToken.Token);

            return Page();
        }

        private async Task SendEmail(string token)
        {
            var link = $"https://{Request.HttpContext.Request.Host.Value}/RecoverPassword?token_s={token}";
            var from = new EmailAddress("no-reply@isolaatti.com", "Isolaatti");
            var to = new EmailAddress(Account.Email, Account.Name);
            var subject = "Cambia tu contraseña de Isolaatti";
            var htmlBody = MailHelper.CreateSingleEmail(from, to, subject,
                $"Abre el enlace para restablecer tu contraseña. {link}",
                string.Format(EmailTemplates.PasswordRecoveryEmail, link));
            await _sendGridClient.SendEmailAsync(htmlBody);
        }
    }
}
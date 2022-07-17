using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
                await Task.Delay(1500);
                return Page();
            }

            EmailFound = true;

            // here create token
            var userToken = new ChangePasswordToken()
            {
                UserId = Account.Id
            };

            if (Db.ChangePasswordTokens.Any(tok => tok.UserId.Equals(Account.Id)))
            {
                var oldToken = Db.ChangePasswordTokens.Single(token => token.UserId.Equals(Account.Id));
                Db.ChangePasswordTokens.Remove(oldToken);
            }

            Db.ChangePasswordTokens.Add(userToken);
            await Db.SaveChangesAsync();

            // send email with link
            await SendEmail(userToken.Id, userToken.Token, Account.Name);

            return Page();
        }

        private async Task SendEmail(Guid id, string token, string username)
        {
            var link =
                $"https://{Request.HttpContext.Request.Host.Value}/RecoverPassword?id={id}&value={HttpUtility.UrlEncode(token)}";
            var from = new EmailAddress("no-reply@isolaatti.com", "Isolaatti");
            var to = new EmailAddress(Account.Email, Account.Name);
            var subject = "Restablecimiento de contraseña de Isolaatti";
            var htmlBody = MailHelper.CreateSingleEmail(from, to, subject,
                $"Abre el enlace para restablecer tu contraseña. {link}",
                string.Format(EmailTemplates.PasswordRecoveryEmail, link, username));
            await _sendGridClient.SendEmailAsync(htmlBody);
        }
    }
}
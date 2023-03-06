using System;
using System.Threading.Tasks;
using Isolaatti.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages
{
    public class RecoverPassword : PageModel
    {
        private readonly DbContextApp Db;

        public RecoverPassword(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        
        [BindProperty (SupportsGet = true, Name = "id")]
        public Guid TokenId { get; set; }
        [BindProperty (SupportsGet = true, Name = "value")]
        public string TokenValue { get; set; }
        [BindProperty]
        public string NewPassword { get; set; }
        [BindProperty]
        public bool NewPasswordInvalid { get; set; }
        public bool UserDoesNotExist { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var changePasswordToken = await Db.ChangePasswordTokens.FindAsync(TokenId);
            if (changePasswordToken == null)
            {
                return NotFound();
            }

            if (changePasswordToken.Token != TokenValue) return Unauthorized();

            if (changePasswordToken.Expires < DateTime.Now.ToUniversalTime())
            {
                Db.ChangePasswordTokens.Remove(changePasswordToken);
                await Db.SaveChangesAsync();
                return NotFound();
            }

            ViewData["id"] = changePasswordToken.Id;
            ViewData["token"] = changePasswordToken.Token;
            return Page();
        }

        public IActionResult OnPost()
        {
            var token = Db.ChangePasswordTokens.Find(TokenId);
            if (token == null) return NotFound();

            if (token.Token != TokenValue) return Unauthorized();
            if (NewPassword == null || NewPassword.Length < 8)
            {
                NewPasswordInvalid = true;
                return Page();
            }

            var user = Db.Users.Find(token.UserId);
            if (user == null)
            {
                UserDoesNotExist = true;
                return Page();
            }
            var passwordHasher = new PasswordHasher<string>();
            user.Password = passwordHasher.HashPassword(user.Email, NewPassword);
            Db.Users.Update(user);
            Db.ChangePasswordTokens.Remove(token);
            Db.SaveChanges();
            return RedirectToPage("LogIn");
        }
    }
}
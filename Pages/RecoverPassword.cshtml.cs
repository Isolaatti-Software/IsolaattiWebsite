using System;
using System.Threading.Tasks;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class RecoverPassword : PageModel
    {
        private readonly DbContextApp Db;

        public RecoverPassword(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }

        public async Task<IActionResult> OnGet([FromQuery] Guid id, [FromQuery] string value)
        {
            var changePasswordToken = await Db.ChangePasswordTokens.FindAsync(id);
            if (changePasswordToken == null)
            {
                return NotFound();
            }

            if (changePasswordToken.Token != value) return Unauthorized();

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

        public IActionResult OnPost([FromForm] Guid id, [FromForm] string tokenStr, [FromForm] string newPassword)
        {
            var token = Db.ChangePasswordTokens.Find(id);
            if (token == null) return NotFound();

            if (token.Token != tokenStr) return Unauthorized();

            var user = Db.Users.Find(token.UserId);
            var passwordHasher = new PasswordHasher<string>();
            user.Password = passwordHasher.HashPassword(user.Email, newPassword);
            Db.Users.Update(user);
            Db.ChangePasswordTokens.Remove(token);
            Db.SaveChanges();
            return RedirectToPage("LogIn");
        }
    }
}
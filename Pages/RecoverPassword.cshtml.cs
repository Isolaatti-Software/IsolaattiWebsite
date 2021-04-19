using System;
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;

namespace isolaatti_API.Pages
{
    public class RecoverPassword : PageModel
    {
        private readonly DbContextApp Db;
        public bool TokenExists = false;
        public bool ExpiredToken = false;

        public RecoverPassword(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        
        public IActionResult OnGet([FromQuery] string token_s)
        {
            UserToken token;
            try
            {
                token = Db.UserTokens.Single(userToken => userToken.Token.Equals(token_s));
            }
            catch (InvalidOperationException)
            {
                return Page();
            }
            TokenExists = true;
            ExpiredToken = DateTime.Compare(token.Expires, DateTime.Now) == -1;
            ViewData["token"] = token.Token;
            return Page();
            
        }

        public IActionResult OnPost([FromForm] string token_s, [FromForm] string newPassword)
        {
            try
            {
                var token = Db.UserTokens.Single(userToken => userToken.Token.Equals(token_s));
                var user = Db.Users.Find(token.UserId);
                var passwordHasher = new PasswordHasher<String>();
                user.Password = passwordHasher.HashPassword(user.Email,newPassword);
                Db.Users.Update(user);
                Db.UserTokens.Remove(token);
                Db.SaveChanges();
                return RedirectToPage("LogIn");
            }
            catch (InvalidOperationException)
            {
                return RedirectToPage("ForgotPassword");
            }

        }
    }
}
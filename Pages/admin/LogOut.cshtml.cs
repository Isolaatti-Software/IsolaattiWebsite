/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System;
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.admin
{
    public class LogOut : PageModel
    {
        private readonly DbContextApp _db;

        public LogOut(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        public IActionResult OnGet()
        {
            var tokenS = Request.Cookies["isolaatti_admin_session"];
            try
            {
                _db.AdminAccountSessionTokens
                    .Remove(_db.AdminAccountSessionTokens.Single(token => token.Token.Equals(tokenS)));
                _db.SaveChanges();
            }
            catch (InvalidOperationException)
            {
                
            }
            Response.Cookies.Delete("isolaatti_admin_session");
            return RedirectToPage("LogIn");
        }
    }
}
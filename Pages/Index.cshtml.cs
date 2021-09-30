/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Index : PageModel
    {
        private readonly DbContextApp _db;

        public Index(DbContextApp dbContextApp, IWebHostEnvironment env)
        {
            _db = dbContextApp;
        }

        public IActionResult OnGet()
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("/s/Welcome");
            
            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
            ViewData["profilePicUrl"] = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);
            
            return Page();
        }
    }
}
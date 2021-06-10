using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class NewProject : PageModel
    {
        private readonly DbContextApp _db;
        public bool LimitOfSongsReached = false;

        public NewProject(DbContextApp dbContextApp, IWebHostEnvironment env)
        {
            _db = dbContextApp;
        }
        public IActionResult OnGet()
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");
            
            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
            LimitOfSongsReached = _db.Songs.Count(song => song.OwnerId.Equals(user.Id)) >= 10;
                    
            return Page();
            
        }
    }
}
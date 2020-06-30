using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.admin
{
    public class Diffusion : PageModel
    {
        private readonly DbContextApp db;
        public Diffusion(DbContextApp _dbContextApp)
        {
            db = _dbContextApp;
        }
        public IActionResult OnGet()
        {
            var username = Request.Cookies["name"];
            var password = Request.Cookies["password"];

            if (username == null || password == null) return RedirectToPage("LogIn");
            if (!db.AdminAccounts.Any(ac => ac.name.Equals(username))) return RedirectToPage("LogIn");
            var account = db.AdminAccounts.Single(ac => ac.name.Equals(username));
            if (!account.password.Equals(password)) return RedirectToPage("LogIn");
            
            // here is safe, the credentials are correct
            
            
            return Page();
        }
    }
}
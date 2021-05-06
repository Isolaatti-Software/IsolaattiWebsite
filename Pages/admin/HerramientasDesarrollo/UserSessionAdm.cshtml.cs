using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.admin.HerramientasDesarrollo
{
    public class UserSessionAdm : PageModel
    {
        private readonly DbContextApp _db;

        public UserSessionAdm(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        public IActionResult OnGet()
        {
            var tokenOnCookie = Request.Cookies["isolaatti_admin_session"];
            if(tokenOnCookie == null) return RedirectToPage("LogIn");
            
            var adminAccounts = new AdminAccounts(_db);
            var user = adminAccounts.ValidateSessionToken(tokenOnCookie);
            if(user == null) return RedirectToPage("LogIn");
            
            // data binding here
            ViewData["username"] = user.name;

            return Page();
        }
    }
}
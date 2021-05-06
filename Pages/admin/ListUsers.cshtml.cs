using System.Collections.Generic;
using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.admin
{
    public class ListUsers : PageModel
    {
        private readonly DbContextApp _db;
        public List<User> Users;

        public ListUsers(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        public IActionResult OnGet([FromQuery] string queryS)
        {
            var tokenOnCookie = Request.Cookies["isolaatti_admin_session"];
            if(tokenOnCookie == null) return RedirectToPage("LogIn");
            
            var adminAccounts = new AdminAccounts(_db);
            var user = adminAccounts.ValidateSessionToken(tokenOnCookie);
            if(user == null) return RedirectToPage("LogIn");
            
            // data binding here
            ViewData["username"] = user.name;
            if (queryS != null)
            {
                Users = _db.Users.Where(user => user.Email.Equals(queryS)).ToList();
            }
            else
            {
                Users = _db.Users.ToList().TakeLast(100).ToList();
            }
            
            return Page();
        }
    }
}
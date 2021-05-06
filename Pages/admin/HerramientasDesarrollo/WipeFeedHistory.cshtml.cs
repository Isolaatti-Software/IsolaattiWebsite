using System;
using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.admin.HerramientasDesarrollo
{
    public class WipeFeedHistory : PageModel
    {
        private readonly DbContextApp _db;

        public WipeFeedHistory (DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        public IActionResult OnGet(string status="", int affectedUser = -1)
        {
            var tokenOnCookie = Request.Cookies["isolaatti_admin_session"];
            if(tokenOnCookie == null) return RedirectToPage("LogIn");
            
            var adminAccounts = new AdminAccounts(_db);
            var user = adminAccounts.ValidateSessionToken(tokenOnCookie);
            if(user == null) return RedirectToPage("LogIn");
            
            // data binding here
            ViewData["username"] = user.name;
            ViewData["status"] = status;
            ViewData["affectedUser"] = affectedUser; 
            return Page();
        }

        public IActionResult OnPost([FromForm] int userId)
        {
            var tokenOnCookie = Request.Cookies["isolaatti_admin_session"];
            if(tokenOnCookie == null) return RedirectToPage("LogIn");
            
            var adminAccounts = new AdminAccounts(_db);
            var user = adminAccounts.ValidateSessionToken(tokenOnCookie);
            if(user == null) return RedirectToPage("LogIn");
            
            // data binding here
            ViewData["username"] = user.name;
            
            // wipe feed history here
            if(!_db.Users.Any(item => item.Id.Equals(userId)))
                return RedirectToPage("/admin/HerramientasDesarrollo/WipeFeedHistory", new
                {
                    status = "user_not_found",
                    affectedUser = userId
                });
            try
            {
                var history = _db.UserSeenPostHistories
                    .Where(item => item.UserId.Equals(userId));
                _db.UserSeenPostHistories.RemoveRange(history);
                _db.SaveChanges();
            }
            catch (ArgumentNullException)
            {
                return RedirectToPage("/admin/HerramientasDesarrollo/WipeFeedHistory", new
                {
                    status = "error"
                });
            }
            
            return RedirectToPage("/admin/HerramientasDesarrollo/WipeFeedHistory", new
            {
                status = "ok",
                affectedUser = userId
            });
        }
    }
}
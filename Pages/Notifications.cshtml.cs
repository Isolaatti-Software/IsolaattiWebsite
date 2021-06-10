using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Notifications : PageModel
    {
        private readonly DbContextApp db;
        public Notifications(DbContextApp dbContext)
        {
            db = dbContext;
        }

        public IQueryable<Notification> UserNotifications;
        public IActionResult OnGet()
        {
            var accountsManager = new Accounts(db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");
            
            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
                    
            // Get user notifications
            UserNotifications = db.Notifications.Where(notification => notification.UserId.Equals(user.Id))
                .OrderByDescending(notification => notification.Id);
            return Page();
        }
    }
}
using System;
using System.Linq;
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
            var email = Request.Cookies["isolaatti_user_name"];
            var password = Request.Cookies["isolaatti_user_password"];

            if (email == null || password == null)
            {
                return RedirectToPage("LogIn");
            }

            try
            {
                var user = db.Users.Single(user => user.Email.Equals(email));
                if (user.Password.Equals(password))
                {
                    if (!user.EmailValidated)
                        return RedirectToPage("LogIn", new
                        {
                            username = email,
                            notVerified = true
                        });
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
            catch (InvalidOperationException)
            {
                return RedirectToPage("LogIn");
            }
            
            return RedirectToPage("LogIn", new
            {
                badCredential = true,
                username = email
            });
        }
    }
}
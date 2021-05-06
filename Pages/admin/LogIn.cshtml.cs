/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System;
using System.Linq;
using isolaatti_API.isolaatti_lib;
using Microsoft.AspNetCore.Mvc.RazorPages;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Pages.admin
{
    public class LogIn : PageModel
    {
        private readonly DbContextApp db;
        public bool AccountError;
        public bool ShowAlert;

        public LogIn(DbContextApp _dbContext)
        {
            db = _dbContext;
        }
        public void OnGet(bool accounterror=false, bool loginagain=false)
        {
            // this is not optimal, change it as soon as possible
            if (!db.AdminAccounts.Any(ac => ac.name.Equals("admin")))
            {
                CreateDefaultAccount();
            }

            if (loginagain)
            {
                ShowAlert = true;
                ViewData["AlertContent"] = "Your password was changed, please log in again";
            }

            if (accounterror)
            {
                ShowAlert = true;
                ViewData["AlertContent"] = "Error, please try again";
            }
        }

        public IActionResult OnPost(string email, string password)
        {
            if (password == null) return Page();
            
            var adminAccounts = new AdminAccounts(db);
            var adminAccount = db.AdminAccounts.Single(account => account.email.Equals(email));
            if(adminAccount == null) return RedirectToPage("/admin/LogIn", new {loginagain = false});
            
            var token = adminAccounts.CreateSessionToken(adminAccount.Id, password);
            if(token == null) return RedirectToPage("/admin/LogIn", new {loginagain = false});
            
            Response.Cookies.Append("isolaatti_admin_session",token,new CookieOptions() {
                Expires = new DateTimeOffset(DateTime.Today.AddMonths(1))
            });
            return RedirectToPage("AdminPortal");
        }
        
        /*
         * This method is called if the default account (username: admin, password: password) doesn't exist'.
         * It assumes that there can only be an admin account named "admin"
         */
        private void CreateDefaultAccount()
        {
            var adminAccounts = new AdminAccounts(db);
            adminAccounts.MakeAccount("admin", "change.me@example.com", "default");
        }
    }
}
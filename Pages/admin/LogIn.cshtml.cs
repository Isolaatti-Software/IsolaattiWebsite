/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using isolaatti_API.Models;

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
        
        /*
         * This method is called if the default account (username: admin, password: password) doesn't exist'.
         * It assumes that there can only be an admin account named "admin"
         */
        private void CreateDefaultAccount()
        {
            var newAccount = new AdminAccount()
            {
                name="admin",
                password = "default",
                email = "change.me@example.com"
            };
            db.AdminAccounts.Add(newAccount);
            db.SaveChanges();
        }
    }
}
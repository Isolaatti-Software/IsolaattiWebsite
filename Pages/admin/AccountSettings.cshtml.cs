/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Ocsp;

namespace isolaatti_API.Pages.admin
{
    public class AccountSettings : PageModel
    {
        private readonly DbContextApp db;

        public bool WantToShowAlert = false;

        private const int ALERT_PASSWORD_CHANGED = 1;
        private const int ALERT_EMAIL_CHANGE = 2;

        public AccountSettings(DbContextApp _dbContext)
        {
            db = _dbContext;
        }
        public IActionResult OnGet()
        {
            return LogIn();
        }

        public IActionResult OnPost(string action, string newEmail="", string currentPassword="", string newPassword="", string newPasswordConfirm="")
        {
            var username = Request.Cookies["name"];
            var password = Request.Cookies["password"];
            AdminAccount adminAccount = db.AdminAccounts.Single(ac => ac.name.Equals(username));
            switch (action)
            {
                case "changePassword":
                    if (ChangePassword(adminAccount, currentPassword, newPassword, newPasswordConfirm))
                    {
                        ShowAlert(ALERT_PASSWORD_CHANGED);
                        Response.Cookies.Append("password",newPassword);
                        return RedirectToPage("LogIn", new {loginagain = true});
                    }
                    break;
                case "changeEmailAddress":
                    if(ChangeEmail(adminAccount,newEmail)) ShowAlert(ALERT_EMAIL_CHANGE);
                    break;
            }
            return LogIn();
        }

        public IActionResult LogIn()
        {
            var username = Request.Cookies["name"];
            var password = Request.Cookies["password"];
            
            // user does not exist
            if (!db.AdminAccounts.Any(ac => ac.name.Equals(username))) return RedirectToPage("LogIn");
            
            // password for that user is not correct
            if (!db.AdminAccounts.Single(ac => ac.name.Equals(username)).password.Equals(password))
                return RedirectToPage("LogIn");
            
            // credentials are correct
            ViewData["username"] = username;
            return Page();
        }

        private bool ChangePassword(AdminAccount adminAccount,string currentPassword, string newPassword, string newPasswordConfirm)
        {
            
            if (!currentPassword.Equals(adminAccount.password)) return false;
            if (!newPassword.Equals(newPasswordConfirm)) return false;
            
            adminAccount.password = newPassword;
            db.AdminAccounts.Update(adminAccount);
            db.SaveChanges();
            return true;
        }

        private bool ChangeEmail(AdminAccount account,string newEmail)
        {
            if (newEmail.Equals("")) return false;
            account.email = newEmail;
            db.AdminAccounts.Update(account);
            db.SaveChanges();
            return true;
        }

        private void ShowAlert(int whatOf)
        {
            WantToShowAlert = true;
            switch (whatOf)
            {
                case ALERT_EMAIL_CHANGE: ViewData["alertContent"] = "Email has been changed";
                    break;
                case ALERT_PASSWORD_CHANGED: ViewData["alertContent"] = "Password changed successfully";
                    break;
            }
        }
    }
}
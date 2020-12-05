using System;
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.admin
{
    public class DeleteUserAccount : PageModel
    {
        private DbContextApp db;
        public bool Error = false;
        public bool Post = false;
        public bool BadAuthAdmin = false;
        public bool AccountDeleted = false;

        public DeleteUserAccount(DbContextApp db)
        {
            this.db = db;
        }
        public IActionResult OnGet(int id)
        {
            var username = Request.Cookies["name"];
            var password = Request.Cookies["password"];

            if (username == null || password == null) return RedirectToPage("LogIn");
            if (!db.AdminAccounts.Any(ac => ac.name.Equals(username))) return RedirectToPage("LogIn");
            var account = db.AdminAccounts.Single(ac => ac.name.Equals(username));
            if (!account.password.Equals(password)) return RedirectToPage("LogIn");
            
            // here is safe, the credentials are correct
            ViewData["username"] = account.name;
            
            try
            {
                var userToDelete = db.Users.Find(id);
                ViewData["name"] = userToDelete.Name;
                ViewData["id"] = userToDelete.Id;
            }
            catch (Exception e)
            {
                ViewData["error"] = e.ToString();
                Error = true;
            }
            
            return Page();
        }

        public IActionResult OnPost(int id, int adminId,string adminPassword)
        {
            var username = Request.Cookies["name"];
            var password = Request.Cookies["password"];

            if (username == null || password == null) return RedirectToPage("LogIn");
            if (!db.AdminAccounts.Any(ac => ac.name.Equals(username))) return RedirectToPage("LogIn");
            var account = db.AdminAccounts.Single(ac => ac.name.Equals(username));
            if (!account.password.Equals(password)) return RedirectToPage("LogIn");
            
            // here is safe, the credentials are correct
            ViewData["username"] = account.name;
            
            Post = true;
            if (db.AdminAccounts.Find(adminId).password.Equals(adminPassword))
            {
                try
                {
                    db.Users.Remove(db.Users.Find(id));
                    db.SaveChanges();
                    AccountDeleted = true;
                }
                catch (Exception e)
                {
                    return StatusCode(404);
                }
                
            }
            else
            {
                BadAuthAdmin = true;
            }

            return Page();
        }
    }
}
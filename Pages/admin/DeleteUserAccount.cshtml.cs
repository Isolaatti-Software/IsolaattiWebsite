using System;
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
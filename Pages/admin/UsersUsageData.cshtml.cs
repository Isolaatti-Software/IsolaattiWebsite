using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;

namespace isolaatti_API.Pages.admin
{
    public class UsersUsageData : PageModel
    {
        private readonly DbContextApp db;

        public IEnumerable<UserUsageData> _usageData;

        public UsersUsageData(DbContextApp _dbContextApp)
        {
            db = _dbContextApp;
        }
        public IActionResult OnGet()
        {
            var username = Request.Cookies["name"];
            var password = Request.Cookies["password"];

            if (username == null || password == null) return RedirectToPage("LogIn");
            if (!db.AdminAccounts.Any(ac => ac.name.Equals(username))) return RedirectToPage("LogIn");
            var account = db.AdminAccounts.Single(ac => ac.name.Equals(username));
            if (!account.password.Equals(password)) return RedirectToPage("LogIn");
            
            // here is safe, the credentials are correct

            
            ViewData["username"] = account.name;
            _usageData = db.UsageData.ToArray();

            return Page();
        }
        
    }
}
using System.Collections.Generic;
using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.admin
{
    public class Reports : PageModel
    {
        private readonly DbContextApp db;
        public List<PostReport> PostReports;
        public List<CommentReport> CommentReports;

        public Reports(DbContextApp dbContext)
        {
            db = dbContext;
        }
        
        public IActionResult OnGet(string status="")
        {
            var tokenOnCookie = Request.Cookies["isolaatti_admin_session"];
            if(tokenOnCookie == null) return RedirectToPage("LogIn");
            
            var adminAccounts = new AdminAccounts(db);
            var user = adminAccounts.ValidateSessionToken(tokenOnCookie);
            if(user == null) return RedirectToPage("LogIn");
            
            // data binding here
            ViewData["username"] = user.name;
            
            // posts reports
            PostReports = db.PostReports.ToList();
            CommentReports = db.CommentReports.ToList();

            ViewData["status"] = status;
            
            return Page();
        }
    }
}
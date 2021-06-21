using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class PostEditor : PageModel
    {
        private readonly DbContextApp _db;
        public bool Edit = false;
        public long EditPostId = 0;

        public PostEditor(DbContextApp dbContextApp, IWebHostEnvironment env)
        {
            _db = dbContextApp;
        }
        public IActionResult OnGet(bool edit = false, long postId = 0)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");
            Edit = edit;
            EditPostId = postId;
            
            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
            return Page();
        }
    }
}
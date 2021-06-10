using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.PublicContent
{
    public class PublicThreadViewer : PageModel
    {
        private readonly DbContextApp _db;
        
        public PublicThreadViewer(DbContextApp dbContext)
        {
            _db = dbContext;
        }
        
        public IActionResult OnGet(long id)
        {
            var post = _db.SimpleTextPosts.Find(id);
            if (post == null) return NotFound();
            if (post.Privacy != 3) return NotFound();

            ViewData["threadId"] = post.Id;

            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user != null)
            {
                return RedirectToPage($"/Threads",new
                {
                    id = id
                });
            }
            
            return Page();
        }
        
    }
}
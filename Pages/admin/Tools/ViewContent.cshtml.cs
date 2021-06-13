using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.admin.Tools
{
    public class ViewContent : PageModel
    {
        private readonly DbContextApp db;

        public ViewContent(DbContextApp dbContext)
        {
            db = dbContext;
        }
        public IActionResult OnGet(long postId = 0, long commentId = 0)
        {
            var tokenOnCookie = Request.Cookies["isolaatti_admin_session"];
            if(tokenOnCookie == null) return RedirectToPage("LogIn");
            
            var adminAccounts = new AdminAccounts(db);
            var user = adminAccounts.ValidateSessionToken(tokenOnCookie);
            if(user == null) return RedirectToPage("LogIn");
            
            // data binding here
            ViewData["username"] = user.name;

            if ((postId == 0 && commentId == 0) || (postId > 0 && commentId > 0))
            {
                return NotFound();
            }

            if (postId > 0)
            {
                var post = db.SimpleTextPosts.Find(postId);
                if (post == null)
                {
                    return NotFound();
                }

                ViewData["type"] = "Publicación";
                ViewData["audioUrl"] = post.AudioAttachedUrl;
                ViewData["content"] = post.TextContent;
            } else if (commentId > 0)
            {
                var comment = db.Comments.Find(commentId);
                if (comment == null)
                {
                    return NotFound();
                }

                ViewData["audioUrl"] = comment.AudioUrl;
                ViewData["type"] = "Comentario";
                ViewData["content"] = comment.TextContent;
            }

            return Page();
        }
    }
}
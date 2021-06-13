using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.Reports
{
    public class ReportPostOrComment : PageModel
    {
        private readonly DbContextApp _db;
        
        public ReportPostOrComment(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        public IActionResult OnGet(long postId = 0,long commentId = 0)
        {
            // as 0 is default value, it can know that user didn't specified anything
            // both parameters cannot be more than 0, only one can be
            if ((postId == 0 && commentId == 0) || (postId > 0 && commentId > 0))
            {
                return NotFound();
            }
            
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("GetStarted");
            
            // here it's known that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
            
            // See what kind of content will be reported. Parameters are obligatory and cannot be defined by the user
            // on this page, but only when clicking on a "report link" on a post or comment
            
            // As long as a post id is not 0 or null it means that user wants to report a post, same for comment
            if (postId > 0)
            {
                ViewData["type"] = 1;
                ViewData["contentId"] = postId;
            }
            if (commentId > 0)
            {
                ViewData["type"] = 2;
                ViewData["contentId"] = commentId;
            }

            return Page();
        }

        public IActionResult OnPost(int typeOfReport,long id, int category, string userReason)
        {
            switch (typeOfReport)
            {
                case 1:
                    if (!_db.SimpleTextPosts.Any(post => post.Id.Equals(id)))
                    {
                        return NotFound();
                    }
                    
                    var postReport = new PostReport()
                    {
                        Category = category,
                        PostId = id,
                        UserReason = userReason
                    };
                    
                    _db.PostReports.Add(postReport);
                    _db.SaveChanges();
                    return RedirectToPage("/Reports/ThankYou");
                    break;
                case 2:
                    if (!_db.Comments.Any(comment => comment.Id.Equals(id)))
                    {
                        return NotFound();
                    }
                    var commentReport = new CommentReport()
                    {
                        Category = category,
                        CommentId = id,
                        UserReason = userReason
                    };
                    _db.CommentReports.Add(commentReport);
                    _db.SaveChanges();
                    return RedirectToPage("/Reports/ThankYou");
                    break;
                default: return  NotFound();
            }
        }
    }
}
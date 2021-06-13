using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Pages.admin.Controllers
{
    [Route("/adminControllers/[controller]")]
    public class DeletePost : Controller
    {
        private readonly DbContextApp _db;

        public DeletePost(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        [HttpPost]
        public IActionResult Index(long postId)
        {
            var sessionTokenFromRequest = Request.Cookies["isolaatti_admin_session"];
            if (sessionTokenFromRequest == null) return Unauthorized("Token is not present");
            
            var adminAccounts = new AdminAccounts(_db);
            var user = adminAccounts.ValidateSessionToken(sessionTokenFromRequest);
            if (user == null) return Unauthorized("Token is invalid");

            var postToDelete = _db.SimpleTextPosts.Find(postId);
            if (postToDelete == null)
            {
                return RedirectToPage("/admin/Reports", new
                {
                    status = "entity_not_found"
                });
            }

            _db.SimpleTextPosts.Remove(postToDelete);
            
            // deletes every report about this post
            var reports = _db.PostReports.Where(report => report.PostId.Equals(postId));
            _db.PostReports.RemoveRange(reports);
            
            _db.SaveChanges();
            
            return RedirectToPage("/admin/Reports", new
            {
                status = "successfully_deleted"
            });
        }

        [Route("DeleteComment")]
        [HttpPost]
        public IActionResult Comment(long commentId)
        {
            var sessionTokenFromRequest = Request.Cookies["isolaatti_admin_session"];
            if (sessionTokenFromRequest == null) return Unauthorized("Token is not present");
            
            var adminAccounts = new AdminAccounts(_db);
            var user = adminAccounts.ValidateSessionToken(sessionTokenFromRequest);
            if (user == null) return Unauthorized("Token is invalid");

            var commentToDelete = _db.Comments.Find(commentId);
            if (commentToDelete == null)
            {
                return RedirectToPage("/admin/Reports", new
                {
                    status = "entity_not_found"
                });
            }

            _db.Comments.Remove(commentToDelete);
            
            // deletes every report about this post
            var reports = _db.CommentReports.Where(report => report.CommentId.Equals(commentId));
            _db.CommentReports.RemoveRange(reports);
            
            _db.SaveChanges();
            
            return RedirectToPage("/admin/Reports", new
            {
                status = "successfully_deleted"
            });
        }
    }
}
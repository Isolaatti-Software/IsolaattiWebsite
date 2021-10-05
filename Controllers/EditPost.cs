using System;
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class EditPost : ControllerBase
    {
        private readonly DbContextApp Db;

        public EditPost(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        

        [HttpPost]
        [Route("Delete")]
        public IActionResult DeletePost([FromForm] string sessionToken, [FromForm] Guid postId)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = Db.SimpleTextPosts.Find(postId);
            if (!post.UserId.Equals(user.Id)) return Unauthorized("You cannot delete a post that is not yours");
            
            // Yep, here I can delete the post
            Db.SimpleTextPosts.Remove(post);
            var commentsOfPost = Db.Comments.Where(comment => comment.SimpleTextPostId == post.Id).ToList();
            Db.Comments.RemoveRange(commentsOfPost);

            var likesOfPost = Db.Likes.Where(like => like.PostId == post.Id).ToList();
            Db.Likes.RemoveRange(likesOfPost);
            
            Db.SaveChanges();
            
            return Ok("Post deleted");
        }
    }
}
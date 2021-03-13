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
        [Route("TextContent")]
        public IActionResult EditTextContent([FromForm] int userId, [FromForm] string password, 
            [FromForm] long postId, [FromForm] string newContent)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User was not found");
            if (!user.Password.Equals(password)) return Unauthorized("Password is not correct");

            var post = Db.SimpleTextPosts.Find(postId);
            if (!post.UserId.Equals(user.Id)) return Unauthorized("You cannot edit a post that is not yours.");
            
            // Yep, here I can edit post
            post.TextContent = newContent;
            Db.SimpleTextPosts.Update(post);
            Db.SaveChanges();
            
            return Ok(post);
        }

        [HttpPost]
        [Route("Delete")]
        public IActionResult DeletePost([FromForm] int userId, [FromForm] string password, 
            [FromForm] long postId)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User was not found");
            if (!user.Password.Equals(password)) return Unauthorized("Password is not correct");

            var post = Db.SimpleTextPosts.Find(postId);
            if (!post.UserId.Equals(user.Id)) return Unauthorized("You cannot delete a post that is not yours");
            
            // Yep, here I can delete the post
            Db.SimpleTextPosts.Remove(post);
            Db.SaveChanges();
            
            return Ok("Post deleted");
        }

        [HttpPost]
        [Route("ChangePrivacy")]
        public IActionResult ChangePrivacy([FromForm] int userId, [FromForm] string password, 
            [FromForm] long postId, [FromForm] int privacyNumber)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User was not found");
            if (!user.Password.Equals(password)) return Unauthorized("Password is not correct");
            
            var post = Db.SimpleTextPosts.Find(postId);
            if (!post.UserId.Equals(user.Id)) return Unauthorized("You cannot change the privacy of a post that is not yours");
            
            // Yep, here I can change the privacy of the post
            post.Privacy = privacyNumber;
            Db.SimpleTextPosts.Update(post);
            Db.SaveChanges();
            
            return Ok(post);
        }
    }
}
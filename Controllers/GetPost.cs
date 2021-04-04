using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class GetPost : ControllerBase
    {
        private readonly DbContextApp _db;

        public GetPost(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        } 
        
        [HttpPost]
        public IActionResult Index([FromForm] int userId, [FromForm] string password, [FromForm] long postId)
        {
            var user = _db.Users.Find(userId);
            if (user == null) return NotFound("user not found");
            if (!user.Password.Equals(password)) return Unauthorized("password is not correct");

            var post = _db.SimpleTextPosts.Find(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != user.Id)) return NotFound("post not found");
            var liked = _db.Likes.Any(like => like.PostId.Equals(post.Id) && like.UserId.Equals(user.Id));
            var author = _db.Users.Find(post.UserId).Name;
            return Ok(new ReturningPostsComposedResponse()
            {
                Id = post.Id,
                Liked = liked,
                NumberOfLikes = post.NumberOfLikes,
                Privacy = post.Privacy,
                TextContent = post.TextContent,
                UserId = post.UserId,
                UserName = author
            });
        }
    }
}
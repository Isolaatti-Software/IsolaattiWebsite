using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MakeComment : ControllerBase
    {
        private readonly DbContextApp _db;

        public MakeComment(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        [HttpPost]
        public IActionResult Index([FromForm] int userId, [FromForm] string password, [FromForm] long postId, 
            [FromForm] string content)
        {
            var user = _db.Users.Find(userId);
            if (user == null) return NotFound("user not found");
            if (!user.Password.Equals(password)) return Unauthorized("password is not correct");

            var post = _db.SimpleTextPosts.Find(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != user.Id)) 
                return NotFound("Post does not exist or is private");


            var newComment = new Comment()
            {
                Privacy = 2,
                SimpleTextPostId = post.Id,
                TextContent = content,
                WhoWrote = user.Id
            };
            _db.Comments.Add(newComment);
            _db.SaveChanges();
            return Ok(new ReturningCommentComposedResponse()
            {
                Id = newComment.Id,
                Privacy = newComment.Privacy,
                SimpleTextPostId = newComment.SimpleTextPostId,
                TextContent = newComment.TextContent,
                WhoWrote = newComment.WhoWrote,
                WhoWroteName = user.Name
            });
        }
    }
}
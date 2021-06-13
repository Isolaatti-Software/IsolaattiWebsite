using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
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
        public IActionResult Index([FromForm] string sessionToken, [FromForm] long postId, 
            [FromForm] string content, [FromForm] string audioUrl = null)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = _db.SimpleTextPosts.Find(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != user.Id)) 
                return NotFound("Post does not exist or is private");


            var newComment = new Comment()
            {
                Privacy = 2,
                SimpleTextPostId = post.Id,
                TextContent = content,
                WhoWrote = user.Id,
                TargetUser = post.UserId,
                AudioUrl = audioUrl
            };
            _db.Comments.Add(newComment);
            _db.SaveChanges();
            AsyncDatabaseUpdates.UpdateNumberOfComments(_db, post.Id);
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
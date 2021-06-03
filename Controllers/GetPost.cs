using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
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
        public IActionResult Index([FromForm] string sessionToken, [FromForm] long postId)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

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

        [HttpPost]
        [Route("Comments")]
        public IActionResult GetComments([FromForm] string sessionToken, [FromForm] long postId)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = _db.SimpleTextPosts.Find(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != user.Id))
                return Unauthorized("post does not exist or is private");

            var comments = _db.Comments
                .Where(comment => comment.SimpleTextPostId.Equals(post.Id))
                .ToList()
                .Select(com => new ReturningCommentComposedResponse()
                {
                    Id = com.Id,
                    Privacy = com.Privacy,
                    SimpleTextPostId = com.SimpleTextPostId,
                    TextContent = com.TextContent,
                    WhoWrote = com.WhoWrote,
                    WhoWroteName = _db.Users.Find(com.WhoWrote).Name
                });
            return Ok(comments);
        }

        [HttpPost]
        [Route("PublicThread")]
        public IActionResult PublicThread([FromForm] long id)
        {
            var post = _db.SimpleTextPosts.Find(id);
            if (!(post is {Privacy: 3})) return NotFound();
            var author = _db.Users.Find(post.UserId).Name;
            return Ok(new
            {
                comments = _db.Comments
                    .Where(comment => comment.SimpleTextPostId.Equals(post.Id))
                    .ToList().Select(com => new ReturningCommentComposedResponse()
                    {
                        Id = com.Id,
                        Privacy = com.Privacy,
                        SimpleTextPostId = com.SimpleTextPostId,
                        TextContent = com.TextContent,
                        WhoWrote = com.WhoWrote,
                        WhoWroteName = _db.Users.Find(com.WhoWrote).Name
                    }),
                post = new ReturningPostsComposedResponse()
                {
                    Id = post.Id,
                    NumberOfLikes = post.NumberOfLikes,
                    Privacy = post.Privacy,
                    TextContent = post.TextContent,
                    UserId = post.UserId,
                    UserName = author
                }
            });
        }
    }
}
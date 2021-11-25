using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

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
            return Ok(new ReturningPostsComposedResponse(post)
            {
                UserName = _db.Users.Find(post.UserId).Name,
                Liked = _db.Likes.Any(element => element.PostId == post.Id && element.UserId == user.Id)
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
                    WhoWroteName = _db.Users.Find(com.WhoWrote).Name,
                    AudioUrl = com.AudioUrl,
                    Date = com.Date
                });
            return Ok(comments);
        }

        [HttpPost]
        [Route("PublicThread")]
        public IActionResult PublicThread([FromForm] long id)
        {
            var post = _db.SimpleTextPosts.Find(id);
            if (!(post is { Privacy: 3 })) return NotFound();
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
                        WhoWroteName = _db.Users.Find(com.WhoWrote).Name,
                        AudioUrl = com.AudioUrl,
                        Date = com.Date
                    }),
                post = new ReturningPostsComposedResponse(post)
                {
                    UserName = _db.Users.Find(post.UserId).Name,
                    NumberOfComments = _db.Comments.Count(comment => comment.SimpleTextPostId.Equals(post.Id)),
                    Date = post.Date
                }
            });
        }
    }
}
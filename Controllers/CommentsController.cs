using System.Linq;
using System.Threading.Tasks;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.Classes.ApiEndpointsResponseDataModels;
using isolaatti_API.Models;
using isolaatti_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/Comment")]
    public class CommentsController : ControllerBase
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public CommentsController(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        [Route("Delete")]
        [HttpPost]
        public async Task<IActionResult> Delete([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null)
            {
                return Unauthorized("Token is not valid");
            }

            var comment = await _db.Comments.FindAsync(identification.Id);
            if (comment == null) return NotFound("Comment not found");

            if (comment.WhoWrote != user.Id)
            {
                return Unauthorized("Access denied, cannot delete this comment, it is not yours");
            }


            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
            // updates comments count of the post this comment belongs
            var post = await _db.SimpleTextPosts.FindAsync(comment.SimpleTextPostId);
            if (post != null)
            {
                post.NumberOfComments = _db.Comments.Count(c => c.SimpleTextPostId.Equals(post.Id));
                _db.SimpleTextPosts.Update(post);
                await _db.SaveChangesAsync();
            }

            return Ok("Comment delete successfully");
        }

        [HttpPost]
        [Route("{commentId:long}/Edit")]
        public async Task<IActionResult> EditComment([FromHeader(Name = "sessionToken")] string sessionToken,
            long commentId, MakeCommentModel updatedComment)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null)
            {
                return Unauthorized("Token is not valid");
            }

            var commentToEdit = await _db.Comments.FindAsync(commentId);
            if (commentToEdit == null) return NotFound();

            commentToEdit.Privacy = updatedComment.Privacy;
            commentToEdit.AudioId = updatedComment.AudioId;
            commentToEdit.TextContent = updatedComment.Content;

            _db.Comments.Update(commentToEdit);
            await _db.SaveChangesAsync();


            return Ok(new FeedComment
            {
                AudioId = commentToEdit.AudioId,
                AuthorId = commentToEdit.WhoWrote,
                AuthorName = (await _db.Users.FindAsync(commentToEdit.WhoWrote)).Name,
                Content = commentToEdit.TextContent,
                Id = commentToEdit.Id,
                PostId = commentToEdit.SimpleTextPostId,
                Privacy = commentToEdit.Privacy,
                TargetUserId = commentToEdit.TargetUser,
                TimeStamp = commentToEdit.Date
            });
        }
    }
}
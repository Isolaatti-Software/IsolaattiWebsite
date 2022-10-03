using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/Posting")]
    public class PostingController : ControllerBase
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;
        private readonly NotificationSender _notificationSender;
        private readonly SquadsRepository _squads;

        public PostingController(DbContextApp dbContextApp, IAccounts accounts, NotificationSender notificationSender, SquadsRepository squadsRepository)
        {
            _db = dbContextApp;
            _accounts = accounts;
            _notificationSender = notificationSender;
            _squads = squadsRepository;
        }

        [HttpPost]
        [Route("Make")]
        public async Task<IActionResult> Index([FromHeader(Name = "sessionToken")] string sessionToken,
            MakePostModel post)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            // Let's verify if Squad exists and that user is authorized to post
            if (post.SquadId.HasValue)
            {
                var squad = await _squads.GetSquad(post.SquadId.Value);
                if (squad == null)
                {
                    return NotFound(new
                    {
                        error = "Squad does not exist"
                    });
                }

                if (!await _squads.UserBelongsToSquad(user.Id, post.SquadId.Value))
                {
                    return NotFound(new
                    {
                        error = "User cannot post in this squad"
                    });
                }
            }


            // Yep, here I can create the post
            var newPost = new Post()
            {
                UserId = user.Id,
                TextContent = post.Content,
                Privacy = post.Privacy,
                AudioId = post.AudioId,
                SquadId = post.SquadId
            };



            _db.SimpleTextPosts.Add(newPost);
            await _db.SaveChangesAsync();

            newPost.Liked = false;
            newPost.NumberOfLikes = 0;
            newPost.NumberOfComments = 0;
            newPost.UserName = user.Name;
            newPost.SquadName = _db.Squads.FirstOrDefault(squad => squad.Id.Equals(post.SquadId))?.Name;

            return Ok(newPost);
        }

        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> EditPost([FromHeader(Name = "sessionToken")] string sessionToken,
            EditPostModel editedPost)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var existingPost = await _db.SimpleTextPosts.FindAsync(editedPost.PostId);
            if (existingPost == null) return NotFound("Post not found");
            if (existingPost.UserId != user.Id) return Unauthorized("Post is not yours, cannot edit");


            existingPost.Privacy = editedPost.Privacy;
            existingPost.TextContent = editedPost.Content;
            existingPost.AudioId = editedPost.AudioId;

            _db.SimpleTextPosts.Update(existingPost);
            
            await _db.SaveChangesAsync();

            return Ok(existingPost);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> DeletePost([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = _db.SimpleTextPosts.Find(identification.Id);
            if (!post.UserId.Equals(user.Id)) return Unauthorized("You cannot delete a post that is not yours");

            // Yep, here I can delete the post
            _db.SimpleTextPosts.Remove(post);
            var commentsOfPost = _db.Comments.Where(comment => comment.SimpleTextPostId == post.Id).ToList();
            _db.Comments.RemoveRange(commentsOfPost);

            var likesOfPost = _db.Likes.Where(like => like.PostId == post.Id).ToList();
            _db.Likes.RemoveRange(likesOfPost);

            await _db.SaveChangesAsync();

            return Ok(new DeletePostOperationResult()
            {
                PostId = post.Id,
                Success = true,
                OperationTime = DateTime.Now
            });
        }

        [HttpPost]
        [Route("Post/{postId:long}/Comment")]
        public async Task<IActionResult> MakeComment([FromHeader(Name = "sessionToken")] string sessionToken,
            long postId, MakeCommentModel commentModel)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = await _db.SimpleTextPosts.FindAsync(postId);
            if (post == null) return Unauthorized("Post does not exist");

            if (!post.UserId.Equals(user.Id) && post.Privacy == 1) return Unauthorized("Post is private");

            var commentToMake = new Comment
            {
                TextContent = commentModel.Content,
                WhoWrote = user.Id,
                SimpleTextPostId = post.Id,
                TargetUser = post.UserId,
                Privacy = commentModel.Privacy,
                AudioId = commentModel.AudioId
            };

            _db.Comments.Add(commentToMake);
            await _db.SaveChangesAsync();
            
            if (post.UserId != user.Id)
            {
                await _notificationSender.NotifyUser(post.UserId, new SocialNotification
                {
                    UserId = user.Id,
                    Type = NotificationType.NewComment
                });
                
            }
            await _notificationSender.SendUpdateEvent(commentToMake);
            return Ok(commentToMake);
        }

        [HttpGet]
        [Route("Post/{postId:long}/CommentsNumber")]
        public async Task<IActionResult> GetNumberOfCommentsOfPost(
            [FromHeader(Name = "sessionToken")] string sessionToken, long postId)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = await _db.SimpleTextPosts.FindAsync(postId);
            if (post == null) return Unauthorized("Post does not exist");
            return Ok(post.NumberOfComments);
        }
    }
}
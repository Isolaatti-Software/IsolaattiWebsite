using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.Classes.ApiEndpointsResponseDataModels;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    /*
     * This Controller receives a parameter called "privacy". It is an integer that means:
     * 1: Private
     * 2: Only available for Isolaatti users
     * 3: Available for everyone
     */

    [ApiController]
    [Route("/api/Posting")]
    public class MakePost : ControllerBase
    {
        private readonly DbContextApp _db;

        public MakePost(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        [HttpPost]
        [Route("Make")]
        public async Task<IActionResult> Index([FromHeader(Name = "sessionToken")] string sessionToken,
            MakePostModel post)
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");


            // Yep, here I can create the post
            var newPost = new SimpleTextPost()
            {
                UserId = user.Id,
                TextContent = post.Content,
                Privacy = post.Privacy,
                AudioId = post.AudioId,
                ThemeJson = JsonSerializer.Serialize(post.Theme,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Date = DateTime.Now
            };

            _db.SimpleTextPosts.Add(newPost);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                postData = new FeedPost
                {
                    Id = newPost.Id,
                    Username = (await _db.Users.FindAsync(newPost.UserId)).Name,
                    UserId = newPost.UserId,
                    Liked = _db.Likes.Any(element => element.PostId == newPost.Id && element.UserId == user.Id),
                    Content = newPost.TextContent,
                    NumberOfLikes = newPost.NumberOfLikes,
                    NumberOfComments = newPost.NumberOfComments,
                    Privacy = newPost.Privacy,
                    AudioId = newPost.AudioId,
                    TimeStamp = newPost.Date
                    // the other attributes are null, but they can be useful in the future
                },
                theme = newPost.ThemeJson == null
                    ? null
                    : JsonSerializer.Deserialize<PostTheme>(newPost.ThemeJson,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            });
        }

        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> EditPost([FromHeader(Name = "sessionToken")] string sessionToken,
            EditPostModel editedPost)
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var existingPost = await _db.SimpleTextPosts.FindAsync(editedPost.PostId);
            if (existingPost == null) return NotFound("Post not found");
            if (existingPost.UserId != user.Id) return Unauthorized("Post is not yours, cannot edit");


            existingPost.Privacy = editedPost.Privacy;
            existingPost.TextContent = editedPost.Content;
            existingPost.AudioId = editedPost.AudioId;
            existingPost.ThemeJson = JsonSerializer.Serialize(editedPost.Theme,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            _db.SimpleTextPosts.Update(existingPost);

            // let's reset the history, to make it appear on the users' feed
            var historyOfThisPost =
                _db.UserSeenPostHistories.Where(history => history.PostId.Equals(existingPost.Id));
            _db.UserSeenPostHistories.RemoveRange(historyOfThisPost);

            await _db.SaveChangesAsync();

            return Ok(new
            {
                postData = new FeedPost
                {
                    Id = existingPost.Id,
                    Username = (await _db.Users.FindAsync(existingPost.UserId)).Name,
                    UserId = existingPost.UserId,
                    Liked = _db.Likes.Any(element => element.PostId == existingPost.Id && element.UserId == user.Id),
                    Content = existingPost.TextContent,
                    NumberOfLikes = existingPost.NumberOfLikes,
                    NumberOfComments = existingPost.NumberOfComments,
                    Privacy = existingPost.Privacy,
                    AudioId = existingPost.AudioId,
                    TimeStamp = existingPost.Date
                    // the other attributes are null, but they can be useful in the future
                },
                theme = existingPost.ThemeJson == null
                    ? null
                    : JsonSerializer.Deserialize<PostTheme>(existingPost.ThemeJson,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            });
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> DeletePost([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(sessionToken);
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

            return Ok("Post deleted");
        }

        [HttpPost]
        [Route("Post/{postId:long}/Comment")]
        public async Task<IActionResult> MakeComment([FromHeader(Name = "sessionToken")] string sessionToken,
            long postId, MakeCommentModel commentModel)
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(sessionToken);
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
                AudioUrl = commentModel.AudioUrl,
                Date = DateTime.Now
            };

            _db.Comments.Add(commentToMake);
            await _db.SaveChangesAsync();
            post.NumberOfComments = _db.Comments.Count(comment => comment.SimpleTextPostId == post.Id);
            _db.SimpleTextPosts.Update(post);
            await _db.SaveChangesAsync();

            return Ok(new FeedComment
            {
                AudioUrl = commentToMake.AudioUrl,
                AuthorId = commentToMake.WhoWrote,
                AuthorName = (await _db.Users.FindAsync(commentToMake.WhoWrote)).Name,
                Content = commentToMake.TextContent,
                Id = commentToMake.Id,
                PostId = commentToMake.SimpleTextPostId,
                Privacy = commentToMake.Privacy,
                TargetUserId = commentToMake.TargetUser,
                TimeStamp = commentToMake.Date
            });
        }

        [Route("Comment/Delete")]
        [HttpPost]
        public async Task<IActionResult> Delete([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null)
            {
                return Unauthorized("Token is not valid");
            }

            var comment = _db.Comments.Find(identification.Id);
            if (comment == null) return NotFound("Comment not found");

            if (comment.WhoWrote != user.Id)
            {
                return Unauthorized("Access denied, cannot delete this comment, it is not yours");
            }

            // remove audio if there is any
            if (comment.AudioUrl != null)
            {
                var storage = GoogleCloudBucket.GetInstance();
                storage.DeleteFile(GoogleCloudStorageUrlUtils.GetFileRefFromUrl(comment.AudioUrl));
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
    }
}
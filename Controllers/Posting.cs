using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using isolaatti_API.Classes;
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
        private readonly DbContextApp Db;

        public MakePost(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }

        [HttpPost]
        [Route("Make")]
        public IActionResult Index([FromHeader(Name = "sessionToken")] string sessionToken, MakePostModel post)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");


            // Yep, here I can create the post
            var newPost = new SimpleTextPost()
            {
                UserId = user.Id,
                TextContent = post.Content,
                Privacy = post.Privacy,
                AudioAttachedUrl = post.AudioUrl,
                ThemeJson = JsonSerializer.Serialize(post.Theme,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Date = DateTime.Now
            };

            Db.SimpleTextPosts.Add(newPost);
            Db.SaveChanges();

            return Ok(new ReturningPostsComposedResponse(newPost)
            {
                UserName = Db.Users.Find(newPost.UserId).Name,
                Liked = Db.Likes.Any(element => element.PostId == newPost.Id && element.UserId == user.Id)
            });
        }

        [HttpPost]
        [Route("Edit")]
        public IActionResult EditPost([FromHeader(Name = "sessionToken")] string sessionToken, EditPostModel editedPost)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var existingPost = Db.SimpleTextPosts.Find(editedPost.PostId);
            if (existingPost == null) return NotFound("Post not found");
            if (existingPost.UserId != user.Id) return Unauthorized("Post is not yours, cannot edit");

            // this means user removed or changed audio, so let's delete from firebase
            if ((existingPost.AudioAttachedUrl != null && editedPost.AudioUrl == null)
                || (existingPost.AudioAttachedUrl != editedPost.AudioUrl && existingPost.AudioAttachedUrl != null))
            {
                GoogleCloudBucket.GetInstance()
                    .DeleteFile(Utils.GoogleCloudStorageUrlUtils.GetFileRefFromUrl(existingPost.AudioAttachedUrl));
            }

            existingPost.Privacy = editedPost.Privacy;
            existingPost.TextContent = editedPost.Content;
            existingPost.AudioAttachedUrl = editedPost.AudioUrl;
            existingPost.ThemeJson = JsonSerializer.Serialize(editedPost.Theme,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            Db.SimpleTextPosts.Update(existingPost);

            // let's reset the history, to make it appear on the users' feed
            var historyOfThisPost =
                Db.UserSeenPostHistories.Where(history => history.PostId.Equals(existingPost.Id));
            Db.UserSeenPostHistories.RemoveRange(historyOfThisPost);

            Db.SaveChanges();

            return Ok(new
            {
                postData = new FeedPost
                {
                    Id = existingPost.Id,
                    Username = Db.Users.Find(existingPost.UserId).Name,
                    UserId = existingPost.UserId,
                    Liked = Db.Likes.Any(element => element.PostId == existingPost.Id && element.UserId == user.Id),
                    Content = existingPost.TextContent,
                    NumberOfLikes = existingPost.NumberOfLikes,
                    NumberOfComments = existingPost.NumberOfComments,
                    Privacy = existingPost.Privacy,
                    AudioUrl = existingPost.AudioAttachedUrl,
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
        public IActionResult DeletePost([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = Db.SimpleTextPosts.Find(identification.Id);
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

        [HttpPost]
        [Route("Post/{postId:long}/Comment")]
        public async Task<IActionResult> MakeComment([FromHeader(Name = "sessionToken")] string sessionToken,
            long postId, MakeCommentModel commentModel)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = await Db.SimpleTextPosts.FindAsync(postId);
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

            Db.Comments.Add(commentToMake);
            await Db.SaveChangesAsync();
            post.NumberOfComments = Db.Comments.Count(comment => comment.SimpleTextPostId == post.Id);
            Db.SimpleTextPosts.Update(post);
            await Db.SaveChangesAsync();

            return Ok(new FeedComment
            {
                AudioUrl = commentToMake.AudioUrl,
                AuthorId = commentToMake.WhoWrote,
                AuthorName = (await Db.Users.FindAsync(commentToMake.WhoWrote)).Name,
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
        public IActionResult Delete([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null)
            {
                return Unauthorized("Token is not valid");
            }

            var comment = Db.Comments.Find(identification.Id);
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

            Db.Comments.Remove(comment);
            Db.SaveChanges();
            // updates comments count of the post this comment belongs
            var post = Db.SimpleTextPosts.Find(comment.SimpleTextPostId);
            if (post != null)
            {
                post.NumberOfComments = Db.Comments.Count(c => c.SimpleTextPostId.Equals(post.Id));
                Db.SimpleTextPosts.Update(post);
                Db.SaveChangesAsync();
            }

            return Ok("Comment delete successfully");
        }
    }
}
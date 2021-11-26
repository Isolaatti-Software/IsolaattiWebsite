using System;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
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
    [Route("/api/[controller]")]
    public class MakePost : ControllerBase
    {
        private readonly DbContextApp Db;

        public MakePost(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }

        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken,
            [FromForm] int privacy = 1,
            [FromForm] string content = "Well, this post was made without content. Why? Idk",
            [FromForm] string audioUrl = null,
            [FromForm] string themeJson = null,
            [FromForm] string postId = "")
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            // this means user wants to edit existing post
            if (postId != "")
            {
                var existingPost = Db.SimpleTextPosts.Find(long.Parse(postId));
                if (existingPost == null) return NotFound("Post not found");
                if (existingPost.UserId != user.Id) return Unauthorized("Post is not yours, cannot edit");

                // this means user removed or changed audio, so let's delete from firebase
                if ((existingPost.AudioAttachedUrl != null && audioUrl == null)
                    || (existingPost.AudioAttachedUrl != audioUrl && existingPost.AudioAttachedUrl != null))
                {
                    GoogleCloudBucket.GetInstance()
                        .DeleteFile(Utils.GoogleCloudStorageUrlUtils.GetFileRefFromUrl(existingPost.AudioAttachedUrl));
                }

                existingPost.Privacy = privacy;
                existingPost.TextContent = content;
                existingPost.AudioAttachedUrl = audioUrl;
                existingPost.ThemeJson = themeJson;

                Db.SimpleTextPosts.Update(existingPost);

                // let's reset the history, to make it appear on the users' feed
                var historyOfThisPost =
                    Db.UserSeenPostHistories.Where(history => history.PostId.Equals(existingPost.Id));
                Db.UserSeenPostHistories.RemoveRange(historyOfThisPost);

                Db.SaveChanges();

                return Ok(new ReturningPostsComposedResponse(existingPost)
                {
                    UserName = Db.Users.Find(existingPost.UserId).Name,
                    Liked = Db.Likes.Any(element => element.PostId == existingPost.Id && element.UserId == user.Id)
                });
            }

            // Yep, here I can create the post
            var newPost = new SimpleTextPost()
            {
                UserId = user.Id,
                TextContent = content,
                Privacy = privacy,
                AudioAttachedUrl = audioUrl,
                ThemeJson = themeJson,
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
    }
}
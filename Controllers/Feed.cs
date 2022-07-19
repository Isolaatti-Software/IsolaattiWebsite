using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Feed : ControllerBase
    {
        private readonly DbContextApp Db;
        private readonly IAccounts _accounts;

        public Feed(DbContextApp dbContextApp, IAccounts accounts)
        {
            Db = dbContextApp;
            _accounts = accounts;
        }

        [HttpGet]
        [Route("{lastId:long}/{length:int}")]
        public async Task<IActionResult> Index([FromHeader(Name = "sessionToken")] string sessionToken, long lastId = 0,
            int length = 10)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var likes = Db.Likes.Where(like => like.UserId == user.Id);
            IQueryable<SimpleTextPost> postsQuery;
            if (lastId <= 0)
            {
                postsQuery =
                    from post in Db.SimpleTextPosts.OrderByDescending(post => post.Id)
                    from following in Db.FollowerRelations
                    where following.UserId == user.Id
                          && post.UserId == following.TargetUserId
                          && post.Privacy != 1
                    select post;
                postsQuery = postsQuery.Take(length);
            }
            else
            {
                postsQuery =
                    from post in Db.SimpleTextPosts.OrderByDescending(post => post.Id)
                    from following in Db.FollowerRelations
                    where following.UserId == user.Id
                          && post.UserId == following.TargetUserId
                          && post.Privacy != 1
                          && post.Id < lastId
                    select post;
                postsQuery = postsQuery.Take(length);
            }

            var posts = postsQuery.Select(rawPost => new
            {
                postData = new FeedPost
                {
                    Id = rawPost.Id,
                    Username = rawPost.User.Name,
                    UserId = rawPost.UserId,
                    Liked = likes.Any(l => l.PostId == rawPost.Id),
                    Content = rawPost.TextContent,
                    NumberOfLikes = rawPost.NumberOfLikes,
                    NumberOfComments = rawPost.NumberOfComments,
                    Privacy = rawPost.Privacy,
                    AudioId = rawPost.AudioId,
                    TimeStamp = rawPost.Date
                    // the other attributes are null, but they can be useful in the future
                },
                theme = rawPost.ThemeJson == null
                    ? null
                    : JsonSerializer.Deserialize<PostTheme>(
                        new MemoryStream(System.Text.Encoding.UTF8.GetBytes(rawPost.ThemeJson)),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            }).ToList();

            long lastPostId;
            try
            {
                lastPostId = posts.Last().postData.Id;
            }
            catch (InvalidOperationException)
            {
                lastPostId = -1;
            }

            return Ok(new
            {
                LastPostId = lastPostId,
                MoreContent = posts.Count == length,
                Posts = posts
            });
        }


        [HttpGet]
        [Route("Public")]
        public IActionResult GetPublicFeed(bool mostLiked = false)
        {
            IEnumerable<ReturningPostsComposedResponse> feed;
            if (mostLiked)
            {
                feed = Db.SimpleTextPosts.Where(post => post.Privacy.Equals(3))
                    .OrderByDescending(post => post.NumberOfLikes)
                    .Take(100).ToList().Select(post => new ReturningPostsComposedResponse(post)
                    {
                        UserName = Db.Users.Find(post.UserId).Name
                    });
            }
            else
            {
                feed = Db.SimpleTextPosts.Where(post => post.Privacy.Equals(3))
                    .OrderByDescending(post => post.Date)
                    .Take(100).ToList().Select(post => new ReturningPostsComposedResponse(post)
                    {
                        UserName = Db.Users.Find(post.UserId).Name
                    });
            }

            return Ok(feed);
        }
    }
}
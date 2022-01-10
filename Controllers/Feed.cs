using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using isolaatti_API.Classes;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.Classes.ApiEndpointsResponseDataModels;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Feed : ControllerBase
    {
        private readonly DbContextApp Db;

        public Feed(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }

        [HttpGet]
        [Route("{lastId:long}/{length:int}")]
        public async Task<IActionResult> Index([FromHeader(Name = "sessionToken")] string sessionToken, long lastId = 0,
            int length = 10)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

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

            var posts = postsQuery.ToList().Select(rawPost => new
            {
                postData = new FeedPost
                {
                    Id = rawPost.Id,
                    Username = Db.Users.Find(rawPost.UserId).Name,
                    UserId = rawPost.UserId,
                    Liked = Db.Likes.Any(element => element.PostId == rawPost.Id && element.UserId == user.Id),
                    Content = rawPost.TextContent,
                    NumberOfLikes = rawPost.NumberOfLikes,
                    NumberOfComments = rawPost.NumberOfComments,
                    Privacy = rawPost.Privacy,
                    AudioUrl = rawPost.AudioAttachedUrl,
                    TimeStamp = rawPost.Date
                    // the other attributes are null, but they can be useful in the future
                },
                theme = rawPost.ThemeJson == null ? null : JsonSerializer.Deserialize<PostTheme>(rawPost.ThemeJson)
            }).OrderByDescending(post => post.postData.NumberOfComments).ToList();
            return Ok(new
            {
                LastPostId = posts.Last().postData.Id,
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
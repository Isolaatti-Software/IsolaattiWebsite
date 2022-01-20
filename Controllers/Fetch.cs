using System;
using System.Collections.Generic;
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
    [ApiController]
    [Route("/api/[controller]")]
    public class Fetch : ControllerBase
    {
        private readonly DbContextApp _db;

        public Fetch(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        [HttpGet]
        [Route("MyProfile")]
        public IActionResult GetMyProfile([FromHeader(Name = "sessionToken")] string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            UserPreferences userPreferences;

            try
            {
                userPreferences = JsonSerializer.Deserialize<UserPreferences>(user.UserPreferencesJson);
            }
            catch (JsonException)
            {
                userPreferences = new UserPreferences()
                {
                    EmailNotifications = false,
                    ProfileHtmlColor = "#731D8C"
                };
            }

            var profile = new Profile
            {
                Username = user.Name,
                Email = user.Email,
                Description = user.DescriptionText,
                Color = userPreferences.ProfileHtmlColor ?? "#731D8C",
                AudioUrl = user.DescriptionAudioUrl,
                NumberOfPosts = _db.SimpleTextPosts.Count(post => post.UserId == user.Id),
                ProfilePictureUrl = UrlGenerators.GenerateProfilePictureUrl(user.Id, sessionToken, Request)
            };
            return Ok(profile);
        }

        [HttpGet]
        [Route("UserProfile")]
        public async Task<IActionResult> GetProfile([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var account = await _db.Users.FindAsync(Convert.ToInt32(identification.Id));
            if (account == null) return NotFound();
            UserPreferences userPreferences;

            try
            {
                userPreferences = JsonSerializer.Deserialize<UserPreferences>(user.UserPreferencesJson);
            }
            catch (JsonException)
            {
                userPreferences = new UserPreferences()
                {
                    EmailNotifications = false,
                    ProfileHtmlColor = "#731D8C"
                };
            }

            var profile = new Profile
            {
                Username = user.Name,
                Email = user.Email,
                Description = user.DescriptionText,
                Color = userPreferences.ProfileHtmlColor ?? "#731D8C",
                AudioUrl = user.DescriptionAudioUrl,
                NumberOfPosts = _db.SimpleTextPosts.Count(post => post.UserId == user.Id),
                ProfilePictureUrl = UrlGenerators.GenerateProfilePictureUrl(user.Id, sessionToken, Request)
            };
            return Ok(profile);
        }

        [HttpGet]
        [Route("PostsOfUser/{userId:int}")]
        public IActionResult GetPosts([FromHeader(Name = "sessionToken")] string sessionToken, int userId)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            List<SimpleTextPost> posts;

            if (user.Id == userId)
            {
                posts = _db.SimpleTextPosts
                    .Where(post => post.UserId == user.Id)
                    .OrderByDescending(post => post.Id).ToList();
            }
            else
            {
                var requestedAuthor = _db.Users.Find(userId);
                if (requestedAuthor == null) return NotFound();

                posts = _db.SimpleTextPosts
                    .Where(post => post.UserId == requestedAuthor.Id && post.Privacy != 1)
                    .OrderByDescending(post => post.Id).ToList();
            }


            var response = posts.ToList()
                .Select(post => new
                {
                    postData = new FeedPost
                    {
                        Id = post.Id,
                        Username = _db.Users.Find(post.UserId).Name,
                        UserId = post.UserId,
                        Liked = _db.Likes.Any(element => element.PostId == post.Id && element.UserId == user.Id),
                        Content = post.TextContent,
                        NumberOfLikes = post.NumberOfLikes,
                        NumberOfComments = post.NumberOfComments,
                        Privacy = post.Privacy,
                        AudioUrl = post.AudioAttachedUrl,
                        TimeStamp = post.Date
                        // the other attributes are null, but they can be useful in the future
                    },
                    theme = post.ThemeJson == null
                        ? null
                        : JsonSerializer.Deserialize<PostTheme>(post.ThemeJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                })
                .ToList();

            return Ok(response);
        }

        [HttpGet]
        [Route("Post/{postId:long}")]
        public IActionResult GetPost([FromHeader(Name = "sessionToken")] string sessionToken, long postId)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = _db.SimpleTextPosts.Find(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != user.Id)) return NotFound("post not found");
            return Ok(new
            {
                postData = new FeedPost
                {
                    Id = post.Id,
                    Username = _db.Users.Find(post.UserId).Name,
                    UserId = post.UserId,
                    Liked = _db.Likes.Any(element => element.PostId == post.Id && element.UserId == user.Id),
                    Content = post.TextContent,
                    NumberOfLikes = post.NumberOfLikes,
                    NumberOfComments = post.NumberOfComments,
                    Privacy = post.Privacy,
                    AudioUrl = post.AudioAttachedUrl,
                    TimeStamp = post.Date
                    // the other attributes are null, but they can be useful in the future
                },
                theme = post.ThemeJson == null
                    ? null
                    : JsonSerializer.Deserialize<PostTheme>(post.ThemeJson,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            });
        }

        [HttpGet]
        [Route("Post/{postId:long}/Comments")]
        public IActionResult GetComments([FromHeader(Name = "sessionToken")] string sessionToken, long postId)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = _db.SimpleTextPosts.Find(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != user.Id))
                return Unauthorized("post does not exist or is private");

            var comments = _db.Comments
                .Where(comment => comment.SimpleTextPostId.Equals(post.Id))
                .OrderByDescending(comment => comment.Id)
                .ToList()
                .Select(com => new FeedComment
                {
                    Id = com.Id,
                    Content = com.TextContent,
                    AuthorId = com.WhoWrote,
                    AuthorName = _db.Users.Find(com.WhoWrote).Name,
                    PostId = com.SimpleTextPostId,
                    TargetUserId = com.TargetUser,
                    Privacy = com.Privacy,
                    AudioUrl = com.AudioUrl,
                    TimeStamp = com.Date
                });
            return Ok(comments);
        }

        [HttpPost]
        [Route("PublicThread/{id:long}")]
        public IActionResult PublicThread(long id)
        {
            var post = _db.SimpleTextPosts.Find(id);
            if (!(post is { Privacy: 3 })) return NotFound();
            var author = _db.Users.Find(post.UserId).Name;
            return Ok(new
            {
                comments = _db.Comments
                    .Where(comment => comment.SimpleTextPostId.Equals(post.Id))
                    .ToList().Select(com => new FeedComment
                    {
                        Id = com.Id,
                        Content = com.TextContent,
                        AuthorId = com.WhoWrote,
                        AuthorName = _db.Users.Find(com.WhoWrote).Name,
                        PostId = com.SimpleTextPostId,
                        TargetUserId = com.TargetUser,
                        Privacy = com.Privacy,
                        AudioUrl = com.AudioUrl,
                        TimeStamp = com.Date
                    }),
                post = new FeedPost
                {
                    Id = post.Id,
                    Username = _db.Users.Find(post.UserId).Name,
                    Liked = false,
                    Content = post.TextContent,
                    NumberOfLikes = post.NumberOfLikes,
                    NumberOfComments = post.NumberOfComments,
                    Privacy = post.Privacy,
                    AudioUrl = post.AudioAttachedUrl,
                    TimeStamp = DateTime.Now
                    // the other attributes are null, but they can be useful in the future
                }
            });
        }

        [HttpGet]
        [Route("GetUserProfileImage")]
        public IActionResult GetUserProfileImage(int userId)
        {
            var otherUser = _db.Users.Find(userId);
            if (otherUser == null) return NotFound("User not found");
            if (otherUser.ProfileImageData == null) return Redirect("/res/imgs/user.png");
            return new FileContentResult(otherUser.ProfileImageData, "image/png");
        }

        [HttpGet]
        [Route("IsUserOnline/{userId:int}")]
        public IActionResult IsUserOnline(int userId, [FromHeader(Name = "sessionToken")] string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            return Ok(Hubs.NotificationsHub.Sessions.ContainsValue(userId));
        }

        [HttpGet]
        [Route("rt_con")]
        public IActionResult GetAllRealTimeConnections()
        {
            return Ok(Hubs.NotificationsHub.Sessions);
        }
    }
}
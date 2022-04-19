using System;
using System.IO;
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
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> GetMyProfile([FromHeader(Name = "sessionToken")] string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            UserPreferences userPreferences;

            try
            {
                userPreferences = await JsonSerializer
                    .DeserializeAsync<UserPreferences>(
                        new MemoryStream(System.Text.Encoding.UTF8.GetBytes(user.UserPreferencesJson)));
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
                NumberOfPosts = await _db.SimpleTextPosts.CountAsync(post => post.UserId == user.Id),
                ProfilePictureUrl = UrlGenerators.GenerateProfilePictureUrl(user.Id, sessionToken, Request),
                NumberOfFollowers = user.NumberOfFollowers,
                NumberOfFollowings = user.NumberOfFollowing,
                NumberOfLikes = await _db.Likes.CountAsync(like => like.TargetUserId == user.Id)
            };
            return Ok(profile);
        }

        [HttpPost]
        [Route("UserProfile")]
        public async Task<IActionResult> GetProfile([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var account = await _db.Users.FindAsync(Convert.ToInt32(identification.Id));
            if (account == null) return NotFound();
            UserPreferences userPreferences;

            try
            {
                userPreferences = JsonSerializer.Deserialize<UserPreferences>(account.UserPreferencesJson);
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
                Username = account.Name,
                Email = account.ShowEmail ? account.Email : null,
                Description = account.DescriptionText,
                Color = userPreferences.ProfileHtmlColor ?? "#731D8C",
                AudioUrl = account.DescriptionAudioUrl,
                NumberOfPosts = _db.SimpleTextPosts.Count(post => post.UserId == account.Id),
                ProfilePictureUrl = UrlGenerators.GenerateProfilePictureUrl(account.Id, sessionToken, Request),
                NumberOfFollowers = account.NumberOfFollowers,
                NumberOfFollowings = account.NumberOfFollowing,
                NumberOfLikes = _db.Likes.Count(like => like.TargetUserId == account.Id)
            };
            return Ok(profile);
        }

        [HttpGet]
        [Route("PostsOfUser/{userId:int}/{length:int?}/{lastId:long?}")]
        public async Task<IActionResult> GetPosts([FromHeader(Name = "sessionToken")] string sessionToken, int userId,
            int length = 10, long lastId = -1, bool olderFirst = false)
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            IQueryable<SimpleTextPost> posts;
            var likes = _db.Likes.Where(like => like.UserId == user.Id);
            if (user.Id == userId)
            {
                if (olderFirst)
                {
                    if (lastId < 0)
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post => post.UserId == user.Id).Take(length);
                    }
                    else
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post => post.UserId == user.Id && post.Id > lastId).Take(length);
                    }
                }
                else
                {
                    if (lastId < 0)
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post => post.UserId == user.Id)
                            .OrderByDescending(post => post.Id).Take(length);
                    }
                    else
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post => post.UserId == user.Id && post.Id < lastId)
                            .OrderByDescending(post => post.Id).Take(length);
                    }
                }
            }
            else
            {
                var requestedAuthor = await _db.Users.FindAsync(userId);
                if (requestedAuthor == null) return NotFound();

                if (olderFirst)
                {
                    if (lastId < 0)
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post => post.UserId == requestedAuthor.Id && post.Privacy != 1).Take(length);
                    }
                    else
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post => post.UserId == requestedAuthor.Id && post.Privacy != 1 && post.Id > lastId)
                            .Take(length);
                    }
                }
                else
                {
                    if (lastId < 0)
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post => post.UserId == requestedAuthor.Id && post.Privacy != 1)
                            .OrderByDescending(post => post.Id).Take(length);
                    }
                    else
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post => post.UserId == requestedAuthor.Id && post.Privacy != 1 && post.Id < lastId)
                            .OrderByDescending(post => post.Id).Take(length);
                    }
                }
            }


            var feed = posts
                .Select(post => new
                {
                    postData = new FeedPost
                    {
                        Id = post.Id,
                        Username = post.User.Name,
                        UserId = post.UserId,
                        Liked = likes.Any(element => element.PostId == post.Id && element.UserId == user.Id),
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
                }).ToList();

            long lastPostId;
            try
            {
                lastPostId = feed.Last().postData.Id;
            }
            catch (InvalidOperationException)
            {
                lastPostId = -1;
            }

            return Ok(new
            {
                feed,
                moreContent = feed.Count == length,
                lastId = lastPostId
            });
        }

        [HttpGet]
        [Route("Post/{postId:long}")]
        public async Task<IActionResult> GetPost([FromHeader(Name = "sessionToken")] string sessionToken, long postId)
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = await _db.SimpleTextPosts.FindAsync(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != user.Id)) return NotFound("post not found");
            return Ok(new
            {
                postData = new FeedPost
                {
                    Id = post.Id,
                    Username = (await _db.Users.FindAsync(post.UserId)).Name,
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
        public async Task<IActionResult> GetComments([FromHeader(Name = "sessionToken")] string sessionToken,
            long postId)
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = await _db.SimpleTextPosts.FindAsync(postId);
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

        [HttpGet]
        [Route("PublicThread/{id:long}")]
        public async Task<IActionResult> PublicThread(long id)
        {
            var post = await _db.SimpleTextPosts.FindAsync(id);
            if (!post.Privacy.Equals(3)) return NotFound();
            var author = (await _db.Users.FindAsync(post.UserId)).Name;
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
                    UserId = post.UserId,
                    Liked = false,
                    Content = post.TextContent,
                    NumberOfLikes = post.NumberOfLikes,
                    NumberOfComments = post.NumberOfComments,
                    Privacy = post.Privacy,
                    AudioUrl = post.AudioAttachedUrl,
                    TimeStamp = DateTime.Now
                    // the other attributes are null, but they can be useful in the future
                },
                theme = post.ThemeJson == null
                    ? null
                    : JsonSerializer.Deserialize<PostTheme>(post.ThemeJson,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            });
        }

        [HttpGet]
        [Route("GetUserProfileImage")]
        public async Task<IActionResult> GetUserProfileImage(int userId)
        {
            var otherUser = await _db.Users.FindAsync(userId);
            if (otherUser == null) return NotFound("User not found");
            if (otherUser.ProfileImageId == null) return Redirect("/res/imgs/user-solid.svg");
            var profileImage = await _db.ProfileImages.FindAsync(otherUser.ProfileImageId);
            if (profileImage == null) return Redirect("/res/imgs/user-solid.svg");
            return new FileContentResult(profileImage.ImageData, "image/png");
        }

        [HttpGet]
        [Route("ProfileImages/{id:guid}.png")]
        public async Task<IActionResult> GetProfileImage(Guid id)
        {
            var image = await _db.ProfileImages.FindAsync(id);
            if (image == null) return NotFound();
            if (image.ImageData == null) return NotFound();
            return new FileContentResult(image.ImageData, "image/png");
        }
    }
}
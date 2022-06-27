using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.Classes.ApiEndpointsResponseDataModels;
using isolaatti_API.Models;
using isolaatti_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Likes : Controller
    {
        private readonly DbContextApp Db;
        private readonly IAccounts _accounts;

        public Likes(DbContextApp dbContextApp, IAccounts accounts)
        {
            Db = dbContextApp;
            _accounts = accounts;
        }

        [HttpPost]
        [Route("LikePost")]
        public async Task<IActionResult> LikePost([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = await Db.SimpleTextPosts.FindAsync(identification.Id);
            if (post == null) return Unauthorized("Post does not exist");
            if (Db.Likes.Any(element => element.UserId == user.Id && element.PostId == identification.Id))
                return Unauthorized("Post already liked");


            Db.Likes.Add(new Like
            {
                PostId = identification.Id,
                UserId = user.Id,
                TargetUserId = post.UserId
            });
            await Db.SaveChangesAsync();
            post.NumberOfLikes = Db.Likes.Count(l => l.PostId == post.Id);
            Db.SimpleTextPosts.Update(post);
            await Db.SaveChangesAsync();


            return Ok(new
            {
                postData = new FeedPost
                {
                    AudioId = post.AudioId,
                    Content = post.TextContent,
                    Description = post.Description,
                    Id = post.Id,
                    Language = post.Language,
                    Liked = true,
                    NumberOfComments = post.NumberOfComments,
                    NumberOfLikes = post.NumberOfLikes,
                    Privacy = post.Privacy,
                    TimeStamp = post.Date,
                    Title = post.Title,
                    Username = (await Db.Users.FindAsync(post.UserId)).Name,
                    UserId = post.UserId
                },
                theme = post.ThemeJson == null
                    ? null
                    : JsonSerializer.Deserialize<PostTheme>(post.ThemeJson,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            });
        }

        [HttpPost]
        [Route("UnLikePost")]
        public async Task<IActionResult> UnLikePost([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = await Db.SimpleTextPosts.FindAsync(identification.Id);
            if (post == null) return Unauthorized("Post does not exist");
            if (!Db.Likes.Any(element => element.UserId == user.Id && element.PostId == identification.Id))
                return Unauthorized("Post cannot be unliked as it is not liked");

            var like = Db.Likes.Single(element => element.PostId == identification.Id && element.UserId == user.Id);
            Db.Likes.Remove(like);
            await Db.SaveChangesAsync();
            post.NumberOfLikes = Db.Likes.Count(l => l.PostId == post.Id);
            Db.SimpleTextPosts.Update(post);
            await Db.SaveChangesAsync();

            return Ok(new
            {
                postData = new FeedPost
                {
                    AudioId = post.AudioId,
                    Content = post.TextContent,
                    Description = post.Description,
                    Id = post.Id,
                    Language = post.Language,
                    Liked = false,
                    NumberOfComments = post.NumberOfComments,
                    NumberOfLikes = post.NumberOfLikes,
                    Privacy = post.Privacy,
                    TimeStamp = post.Date,
                    Title = post.Title,
                    Username = (await Db.Users.FindAsync(post.UserId)).Name,
                    UserId = post.UserId
                },
                theme = post.ThemeJson == null
                    ? null
                    : JsonSerializer.Deserialize<PostTheme>(post.ThemeJson,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            });
        }
    }
}
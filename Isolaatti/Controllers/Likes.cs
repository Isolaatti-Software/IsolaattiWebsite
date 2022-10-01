using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Likes : Controller
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public Likes(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        [HttpPost]
        [Route("LikePost")]
        public async Task<IActionResult> LikePost([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = await _db.SimpleTextPosts.FindAsync(identification.Id);
            if (post == null) return Unauthorized("Post does not exist");
            if (_db.Likes.Any(element => element.UserId == user.Id && element.PostId == identification.Id))
                return Unauthorized("Post already liked");


            _db.Likes.Add(new Like
            {
                PostId = identification.Id,
                UserId = user.Id,
                TargetUserId = post.UserId
            });
            await _db.SaveChangesAsync();
            post.NumberOfLikes = _db.Likes.Count(l => l.PostId == post.Id);
            _db.SimpleTextPosts.Update(post);
            await _db.SaveChangesAsync();


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
                    Username = (await _db.Users.FindAsync(post.UserId)).Name,
                    UserId = post.UserId,
                    SquadId = post.SquadId,
                    SquadName = _db.Squads.Find(post.SquadId) == null ? null : _db.Squads.Find(post.SquadId)?.Name
                }
            });
        }

        [HttpPost]
        [Route("UnLikePost")]
        public async Task<IActionResult> UnLikePost([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = await _db.SimpleTextPosts.FindAsync(identification.Id);
            if (post == null) return Unauthorized("Post does not exist");
            if (!_db.Likes.Any(element => element.UserId == user.Id && element.PostId == identification.Id))
                return Unauthorized("Post cannot be unliked as it is not liked");

            var like = _db.Likes.Single(element => element.PostId == identification.Id && element.UserId == user.Id);
            _db.Likes.Remove(like);
            await _db.SaveChangesAsync();
            post.NumberOfLikes = _db.Likes.Count(l => l.PostId == post.Id);
            _db.SimpleTextPosts.Update(post);
            await _db.SaveChangesAsync();

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
                    Username = (await _db.Users.FindAsync(post.UserId))?.Name,
                    UserId = post.UserId,
                    SquadId = post.SquadId,
                    SquadName = _db.Squads.Find(post.SquadId) == null ? null : _db.Squads.Find(post.SquadId)?.Name
                }
            });
        }
    }
}
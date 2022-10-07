using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
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
            
            
            return Ok(post);
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

            return Ok(post);
        }
    }
}
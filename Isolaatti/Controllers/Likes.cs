using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Likes : Controller
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;
        private readonly NotificationSender _notificationSender;

        public Likes(DbContextApp dbContextApp, IAccounts accounts, NotificationSender notificationSender)
        {
            _db = dbContextApp;
            _accounts = accounts;
            _notificationSender = notificationSender;
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

            var likeDto = new LikeDto
            {
                PostId = post.Id,
                LikesCount = await _db.Likes.CountAsync(like => like.PostId == post.Id)
            };

            try
            {
                var clientId = Guid.Parse(Request.Headers["client-id"]);
                await _notificationSender.SendPostUpdate(post.Id, clientId);
            } catch(FormatException) {}
            
            return Ok(likeDto);
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

            var likeDto = new LikeDto
            {
                PostId = post.Id,
                LikesCount = await _db.Likes.CountAsync(l => l.PostId == post.Id)
            };

            try
            {
                var clientId = Guid.Parse(Request.Headers["client-id"]);
                await _notificationSender.SendPostUpdate(post.Id, clientId);
            } catch(FormatException) {}


            return Ok(likeDto);
        }
    }
}
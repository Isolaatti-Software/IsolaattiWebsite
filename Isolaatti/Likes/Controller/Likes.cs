using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Likes.Data;
using Isolaatti.Likes.Dto;
using Isolaatti.Notifications.Services;
using Isolaatti.Posting.Dto;
using Isolaatti.RealtimeInteraction.Service;
using Isolaatti.Utils;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Likes.Controller
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Likes : ProfileRequiredController
    {
        private readonly DbContextApp _db;
        private readonly NotificationSender _notificationSender;
        private readonly NotificationsService _notifications;

        public Likes(DbContextApp dbContextApp, NotificationSender notificationSender, NotificationsService notifications)
        {
            _db = dbContextApp;
            _notificationSender = notificationSender;
            _notifications = notifications;
        }

        
        [HttpPost]
        [Route("LikePost")]
        public async Task<IActionResult> LikePost(SingleIdentification identification)
        {
            var post = await _db.SimpleTextPosts.FindAsync(identification.Id);
            if (post == null) return Unauthorized("Post does not exist");
            if (_db.Likes.Any(element => element.UserId == Profile.Id && element.PostId == identification.Id))
                return Unauthorized("Post already liked");



            var likeEntity = new Like
            {
                PostId = identification.Id,
                UserId = Profile.Id,
                TargetUserId = post.UserId,
                SquadId = post.SquadId
            };
            _db.Likes.Add(likeEntity);

            await _db.SaveChangesAsync();

            var likeDto = new LikeDto
            {
                PostId = post.Id,
                LikesCount = await _db.Likes.CountAsync(like => like.PostId == post.Id)
            };
            
            if(post.UserId != Profile.Id)
            {
                await _notifications.InsertNewLikeNotification(likeEntity);
            }

            return Ok(likeDto);
        }

        
        [HttpPost]
        [Route("UnLikePost")]
        public async Task<IActionResult> UnLikePost(SingleIdentification identification)
        {
            var post = await _db.SimpleTextPosts.FindAsync(identification.Id);
            if (post == null) return Unauthorized("Post does not exist");
            if (!_db.Likes.Any(element => element.UserId == Profile.Id && element.PostId == identification.Id))
                return Unauthorized("Post cannot be unliked as it is not liked");

            var like = _db.Likes.Single(element => element.PostId == identification.Id && element.UserId == Profile.Id);
            _db.Likes.Remove(like);
            await _db.SaveChangesAsync();

            var likeDto = new LikeDto
            {
                PostId = post.Id,
                LikesCount = await _db.Likes.CountAsync(l => l.PostId == post.Id)
            };

            
            return Ok(likeDto);
        }

        
        [HttpGet]
        [Route("LikedBy/{userId:int}")]
        public async Task<IActionResult> PostsLikedByUser(int userId, long lastId = long.MaxValue)
        {
            var posts =
                from post in _db.SimpleTextPosts
                from like in _db.Likes
                orderby post.Id descending
                where post.Id == like.PostId && like.UserId == userId && post.Id < lastId
                select new PostDto
                {
                    Post = post,
                    Liked = true,
                    UserName = _db.Users.Where(u => u.Id == post.UserId).Select(u => u.Name).FirstOrDefault(),
                    SquadName = _db.Squads.Where(s => s.Id.Equals(post.SquadId)).Select(s => s.Name).FirstOrDefault(),
                    NumberOfComments = _db.Comments.Count(c => c.PostId == post.Id),
                    NumberOfLikes = _db.Likes.Count(l => l.PostId == post.Id)
                };
            posts = posts.Take(15);

            return Ok(posts);

        }

        
        [HttpGet]
        [Route("PostsUser/{authorUserId:int}/LikedOf/{targetUserId:int}")]
        public async Task<IActionResult> PostsOfUserLikedByUserWithId(int authorUserId, int targetUserId, long lastId = long.MaxValue)
        {
            var posts =
                from post in _db.SimpleTextPosts
                from like in _db.Likes
                orderby post.Id descending
                where post.Id == like.PostId && like.UserId == authorUserId && like.TargetUserId == targetUserId &&
                      post.Id < lastId
                select new PostDto
                {
                    Post = post,
                    Liked = true,
                    UserName = _db.Users.Where(u => u.Id == post.UserId).Select(u => u.Name).FirstOrDefault(),
                    SquadName = _db.Squads.Where(s => s.Id.Equals(post.SquadId)).Select(s => s.Name).FirstOrDefault(),
                    NumberOfComments = _db.Comments.Count(c => c.PostId == post.Id),
                    NumberOfLikes = _db.Likes.Count(l => l.PostId == post.Id)
                };
            return Ok(posts);
        }
    }
}
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Likes : ControllerBase
    {
        private readonly DbContextApp Db;

        public Likes(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        
        [HttpPost]
        [Route("LikePost")]
        public IActionResult LikePost([FromForm]string sessionToken, [FromForm] long postId)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var post = Db.SimpleTextPosts.Find(postId);
            if (post == null) return Unauthorized("Post does not exist");
            if (Db.Likes.Any(element => element.UserId == user.Id && element.PostId == postId))
                return Unauthorized("Post already liked");

            var notificationsAdministration = new NotificationsAdministration(Db);
            
            Db.Likes.Add(new Like()
            {
                PostId = postId,
                UserId = user.Id,
                TargetUserId = post.UserId
            });
            post.NumberOfLikes += 1;
            Db.SimpleTextPosts.Update(post);
            Db.SaveChanges();
            
            notificationsAdministration.NewLikesActivityNotification(post.UserId, user.Id, post.Id, post.NumberOfLikes);
            
            return Ok(new ReturningPostsComposedResponse(post)
            {
                UserName = Db.Users.Find(post.UserId).Name,
                NumberOfComments = Db.Comments.Count(comment => comment.SimpleTextPostId.Equals(post.Id)),
                Liked = Db.Likes.Any(element => element.PostId == post.Id && element.UserId == user.Id)
            });
        }

        [HttpPost]
        [Route("UnLikePost")]
        public IActionResult UnLikePost([FromForm]string sessionToken, [FromForm] long postId)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var post = Db.SimpleTextPosts.Find(postId);
            if (post == null) return Unauthorized("Post does not exist");
            if (!Db.Likes.Any(element => element.UserId == user.Id && element.PostId == postId))
                return Unauthorized("Post cannot be unliked as it is not liked");

            var like = Db.Likes.Single(element => element.PostId == postId && element.UserId == user.Id);
            Db.Likes.Remove(like);

            post.NumberOfLikes -= 1;
            Db.SimpleTextPosts.Update(post);
            Db.SaveChanges();

            return Ok(new ReturningPostsComposedResponse(post)
            {
                UserName = Db.Users.Find(post.UserId).Name,
                NumberOfComments = Db.Comments.Count(comment => comment.SimpleTextPostId.Equals(post.Id)),
                Liked = Db.Likes.Any(element => element.PostId == post.Id && element.UserId == user.Id)
            });
        }
    }
}
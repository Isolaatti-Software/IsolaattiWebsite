using System;
using System.Linq;
using System.Threading.Tasks;
using isolaatti_API.Classes;
using isolaatti_API.Hubs;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MakeComment : ControllerBase
    {
        private readonly DbContextApp _db;
        private readonly IHubContext<NotificationsHub> _hubContext;

        public MakeComment(DbContextApp dbContextApp, IHubContext<NotificationsHub> hubContext)
        {
            _db = dbContextApp;
            _hubContext = hubContext;
        }
        
        [HttpPost]
        public async Task<IActionResult> Index([FromForm] string sessionToken, [FromForm] long postId, 
            [FromForm] string content, [FromForm] string audioUrl = null)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = _db.SimpleTextPosts.Find(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != user.Id)) 
                return NotFound("Post does not exist or is private");

            var notificationsAdministration = new NotificationsAdministration(_db);
            var newComment = new Comment()
            {
                Privacy = 2,
                SimpleTextPostId = post.Id,
                TextContent = content,
                WhoWrote = user.Id,
                TargetUser = post.UserId,
                AudioUrl = audioUrl,
                Date = DateTime.Now
            };
            _db.Comments.Add(newComment);

            var numberOfComments = _db.Comments.Count(comment => comment.SimpleTextPostId == postId);
            
            notificationsAdministration
                .NewCommentsActivityNotification(post.UserId, user.Id, postId, 
                    numberOfComments);
            
            _db.SaveChanges();
            
            var sessionsId = Hubs.NotificationsHub.Sessions.Where(element => element.Value.Equals(post.UserId));
            
            foreach (var id in sessionsId)
            {
                await _hubContext.Clients.Client(id.Key)
                    .SendAsync("fetchNotification");
            }
            
            
            await AsyncDatabaseUpdates.UpdateNumberOfComments(_db, post.Id);
            
            return Ok(new ReturningCommentComposedResponse()
            {
                Id = newComment.Id,
                Privacy = newComment.Privacy,
                SimpleTextPostId = newComment.SimpleTextPostId,
                TextContent = newComment.TextContent,
                WhoWrote = newComment.WhoWrote,
                WhoWroteName = user.Name,
                Date = newComment.Date
            });
        }
    }
}
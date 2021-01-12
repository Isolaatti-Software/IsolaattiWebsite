using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeleteNotification : ControllerBase
    {
        private readonly DbContextApp _db;

        public DeleteNotification(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        [HttpPost]
        public IActionResult Index([FromForm] int userId, [FromForm] string password, [FromForm] int notificationId)
        {
            var user = _db.Users.Find(userId);
            if (user == null) return NotFound("User does not exist");
            if (!user.Password.Equals(password)) return Unauthorized();
            var notification = _db.Notifications.Find(notificationId);
            if (notification.UserId != userId) return Unauthorized("This notification is not yours");
            _db.Notifications.Remove(notification);
            _db.SaveChanges();
            return Ok("Notification deleted successfully");
        }

        [Route("All")]
        [HttpPost]
        public IActionResult DeleteAll([FromForm] int userId, [FromForm] string password)
        {
            var user = _db.Users.Find(userId);
            if (user == null) return NotFound("User does not exist");
            if (!user.Password.Equals(password)) return Unauthorized();
            var notifications = _db.Notifications
                .Where(notification => notification.UserId.Equals(userId));
            _db.Notifications.RemoveRange(notifications);
            _db.SaveChanges();
            return Ok();
        }
    }
}
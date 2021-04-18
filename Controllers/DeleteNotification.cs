using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class DeleteNotification : ControllerBase
    {
        private readonly DbContextApp _db;

        public DeleteNotification(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken, [FromForm] int notificationId)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var notification = _db.Notifications.Find(notificationId);
            if (notification.UserId != user.Id) return Unauthorized("This notification is not yours");
            _db.Notifications.Remove(notification);
            
            _db.SaveChanges();
            
            return Ok("Notification deleted successfully");
        }

        [Route("All")]
        [HttpPost]
        public IActionResult DeleteAll([FromForm] string sessionToken, [FromForm] string password)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var notifications = _db.Notifications
                .Where(notification => notification.UserId.Equals(user.Id));
            _db.Notifications.RemoveRange(notifications);
            
            _db.SaveChanges();
            return Ok();
        }
    }
}
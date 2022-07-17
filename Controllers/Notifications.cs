using System.Linq;
using isolaatti_API.Models;
using isolaatti_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class Notifications : ControllerBase
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public Notifications(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        [HttpPost]
        [Route("GetAllNotifications")]
        public IActionResult GetAll([FromForm] string sessionToken)
        {
            var user = _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var notifications =
                _db.SocialNotifications.Where(notification => notification.UserId == user.Id);


            return Ok(notifications.OrderByDescending(notification => notification.TimeSpan).ToList());
        }

        [HttpPost]
        [Route("DeleteNotification")]
        public IActionResult DeleteANotification([FromForm] string sessionToken, [FromForm] long id)
        {
            var user = _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var notification = _db.SocialNotifications.Find(id);
            if (notification == null) return NotFound("Notification not found");
            if (notification.UserId != user.Id) return Unauthorized("This notification is not yours");
            _db.SocialNotifications.Remove(notification);

            _db.SaveChanges();

            return Ok("Notification deleted successfully");
        }

        [Route("Delete/All")]
        [HttpPost]
        public IActionResult DeleteAll([FromForm] string sessionToken)
        {
            var user = _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var notifications = _db.SocialNotifications
                .Where(notification => notification.UserId.Equals(user.Id));
            if (!notifications.Any()) return NotFound("No notifications were found, ok");
            _db.SocialNotifications.RemoveRange(notifications);
            _db.SaveChanges();
            return Ok();
        }

        [Route("MarkAsRead")]
        [HttpPost]
        public IActionResult MarkAsRead([FromForm] string sessionToken)
        {
            var user = _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var notifications = _db.SocialNotifications.Where(notification =>
                notification.UserId.Equals(user.Id) && !notification.Read);

            foreach (var notification in notifications)
            {
                notification.Read = true;
            }

            _db.SocialNotifications.UpdateRange(notifications);
            _db.SaveChanges();
            return Ok();
        }
    }
}
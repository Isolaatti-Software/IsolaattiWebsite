using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GetNotificationsController : ControllerBase
    {
        private readonly DbContextApp db;
        public GetNotificationsController(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        [HttpPost]
        public ActionResult<IEnumerable<Notification>> Index([FromForm]int userId)
        {
            var notifications =
                db.Notifications
                    .Where(notification => notification.UserId.Equals(userId))
                    .ToArray();
            notifications = notifications.OrderByDescending(notification => notification.Id).ToArray();
            return notifications;
        }
    }
}
/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.Collections.Generic;
using System.Linq;
using isolaatti_API.isolaatti_lib;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class GetNotificationsController : ControllerBase
    {
        private readonly DbContextApp db;
        public GetNotificationsController(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        [HttpPost]
        public ActionResult<IEnumerable<Notification>> Index([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var notifications =
                db.Notifications
                    .Where(notification => notification.UserId.Equals(user.Id))
                    .ToArray();
            notifications = notifications.OrderByDescending(notification => notification.Id).ToArray();
            return notifications;
        }
    }
}
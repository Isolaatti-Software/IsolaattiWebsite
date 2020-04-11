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
        public ActionResult<IEnumerable<Notification>> Index(int userId)
        {
            return 
                db.Notifications
                .Where(notification => notification.UserId.Equals(userId))
                .ToArray();

        }
    }
}
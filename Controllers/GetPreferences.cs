using System.Collections.Generic;
using isolaatti_API.Classes;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    public class GetPreferences : Controller
    {
        private readonly DbContextApp db;

        public GetPreferences(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        [HttpPost]
        public ActionResult<UserPreferences> Index(int userId)
        {
            User user = db.Users.Find(userId);
            return new UserPreferences()
            {
                EmailNotifications = user.NotifyByEmail,
                NotifyWhenProcessFinishes = user.NotifyWhenProcessFinishes,
                NotifyWhenProcessStarted = user.NotifyWhenProcessStarted
            };
        }
    }
}
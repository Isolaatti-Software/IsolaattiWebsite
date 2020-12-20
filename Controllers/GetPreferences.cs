/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
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
/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    public class GetPreferences : Controller
    {
        private readonly DbContextApp _db;

        public GetPreferences(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        [HttpPost]
        public ActionResult<UserPreferences> Index([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            return new UserPreferences()
            {
                EmailNotifications = user.NotifyByEmail,
                NotifyWhenProcessFinishes = user.NotifyWhenProcessFinishes,
                NotifyWhenProcessStarted = user.NotifyWhenProcessStarted
            };
        }
    }
}
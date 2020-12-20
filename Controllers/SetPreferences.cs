/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SetPreferences : ControllerBase
    {
        private readonly DbContextApp db;

        public SetPreferences(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }

        [HttpPost]
        public void Index([FromForm] int userId, [FromForm] int what, [FromForm] bool value)
        {
            User user = db.Users.Find(userId);
            switch (what)
            {
                case 0: user.NotifyByEmail = value;
                    break;
                case 1: user.NotifyWhenProcessStarted = value;
                    break;
                case 2: user.NotifyWhenProcessFinishes = value;
                    break;
            }
            db.Users.Update(user);
            db.SaveChanges();
        }
    }
}
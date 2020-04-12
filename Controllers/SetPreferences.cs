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
        public void Index(
            [FromForm] int userId,
            [FromForm] bool notifyByEmail,
            [FromForm] bool notifyWhenProcessStarted,
            [FromForm] bool notifyWhenProcessFinishes)
        {
            User user = db.Users.Find(userId);
            user.NotifyByEmail = notifyByEmail;
            user.NotifyWhenProcessFinishes = notifyWhenProcessFinishes;
            user.NotifyWhenProcessStarted = notifyWhenProcessStarted;
            db.Users.Update(user);
            db.SaveChanges();
        }
    }
}
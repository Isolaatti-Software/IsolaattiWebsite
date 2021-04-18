/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class SetPreferences : ControllerBase
    {
        private readonly DbContextApp db;

        public SetPreferences(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }

        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken, [FromForm] int what, [FromForm] bool value)
        {
            var accountsManager = new Accounts(db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
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
            
            return Ok();
        }
    }
}
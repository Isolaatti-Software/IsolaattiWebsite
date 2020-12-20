/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UpdateTokenController : ControllerBase
    {
        private readonly DbContextApp db;
        public UpdateTokenController(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        [HttpPost]
        public void Index([FromForm]int userId, [FromForm]string token)
        {
            User user = db.Users.Find(userId);
            user.GoogleToken = token;
            db.Users.Update(user);
            db.SaveChanges();
        }
    }
}
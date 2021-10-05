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
    [ApiController]
    [Route("/api/[controller]")]
    public class ChangePassword : ControllerBase
    {
        private readonly DbContextApp Db;

        public ChangePassword(DbContextApp _dbContext)
        {
            Db = _dbContext;
        }
        public IActionResult Index([FromForm] string sessionToken, [FromForm] string newPassword)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            user.Password = newPassword;
            Db.Users.Update(user);
            Db.SaveChanges();
            
            return Ok();
        }
    }
}
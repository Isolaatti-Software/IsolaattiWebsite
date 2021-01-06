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
    [ApiController]
    [Route("[controller]")]
    public class ChangePassword : ControllerBase
    {
        private readonly DbContextApp Db;

        public ChangePassword(DbContextApp _dbContext)
        {
            Db = _dbContext;
        }
        public bool Index([FromForm] int userId, [FromForm] string password, [FromForm] string newPassword)
        {
            var user = Db.Users.Find(userId);
            if (user.Password.Equals(password))
            {
                user.Password = newPassword;
                Db.Users.Update(user);
                Db.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
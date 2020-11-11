using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;
using isolaatti_API;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LogIn : ControllerBase
    {
        private readonly DbContextApp dbContext;
        public LogIn(DbContextApp appDbContext)
        {
            dbContext = appDbContext;
        }
        
        [HttpPost]
        public ActionResult<UserData> Index([FromForm] string email, [FromForm] string password)
        {
            /*
            if(!dbContext.Users.Any(user => user.Email.Equals(email)))
            {
                return new UserData(false);
            }
            User userToEnter = dbContext.Users.Single(user => user.Email.Equals(email));
            if(userToEnter.Password == password)
            {
                return new UserData(userToEnter.Id, dbContext);
            }
            return new UserData(true); //bad password (see UserData class for more info)*/
            Accounts accounts = new Accounts(dbContext);
            return accounts.LogIn(email, password);
        }
    }
}
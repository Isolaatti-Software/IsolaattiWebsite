/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;
using isolaatti_API.isolaatti_lib;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class LogIn : ControllerBase
    {
        private readonly DbContextApp dbContext;
        public LogIn(DbContextApp appDbContext)
        {
            dbContext = appDbContext;
        }
        
        [HttpPost]
        public IActionResult Index([FromForm] string email, [FromForm] string password)
        {
            var user = dbContext.Users.Single(_user => _user.Email.Equals(email));
            if (user == null) return NotFound("User not found");
            
            var accounts = new Accounts(dbContext);
            accounts.DefineHttpRequestObject(Request);
            var tokenObj = accounts.CreateNewToken(user.Id, password);
            if (tokenObj == null) return Unauthorized("Could not get session. Password might be wrong");
            return Ok(tokenObj.Token);
        }

        [Route("Verify")]
        [HttpPost]
        public IActionResult GetUserData([FromForm] string sessionToken)
        {
            var accounts = new Accounts(dbContext);
            var user = accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("token is not valid");
            return Ok("ok");
        }
    }
}
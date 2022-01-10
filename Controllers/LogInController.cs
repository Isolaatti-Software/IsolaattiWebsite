/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System.Linq;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Index(SignInFormModel data)
        {
            var user = dbContext.Users.Single(_user => _user.Email.Equals(data.Email));
            if (user == null) return NotFound("User not found");

            var accounts = new Accounts(dbContext);
            accounts.DefineHttpRequestObject(Request);
            var tokenObj = accounts.CreateNewToken(user.Id, data.Password);
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
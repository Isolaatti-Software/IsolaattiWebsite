/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class GetUserSongsOnQueue : Controller
    {
        private readonly DbContextApp _db;

        public GetUserSongsOnQueue(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        [HttpPost]
        public IActionResult Index([FromForm]string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var elements =
                _db.SongsQueue
                    .Where(element => element.UserId.Equals(user.Id.ToString()) && 
                                      !element.Reserved);
            
            return Ok(elements);
        }

        [HttpPost]
        [Route("Count")]
        public IActionResult Count([FromForm]string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            return Ok(_db.SongsQueue
                .Count(element => element.UserId.Equals(user.Id.ToString()) && 
                                  !element.Reserved));
        }
    }
}
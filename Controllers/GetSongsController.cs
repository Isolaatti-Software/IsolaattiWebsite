/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.Linq;
using isolaatti_API.isolaatti_lib;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class GetSongsController : ControllerBase
    {
        private readonly DbContextApp _db;
        public GetSongsController(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            return Ok(_db.Songs
                .Where(song => song.OwnerId.Equals(user.Id) && !song.IsBeingProcessed));
        }

        [HttpPost]
        [Route("Processing")]
        public IActionResult Processing([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            return Ok(_db.Songs
                .Where(song => song.OwnerId.Equals(user.Id) && song.IsBeingProcessed));
        }
    }
}
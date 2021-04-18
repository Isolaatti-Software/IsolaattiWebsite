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
    public class GetSongInfo : ControllerBase
    {
        private readonly DbContextApp db;

        public GetSongInfo(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken, [FromForm] int id)
        {
            var accountsManager = new Accounts(db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            return Ok(db.Songs.Find(id));
        }
    }
}
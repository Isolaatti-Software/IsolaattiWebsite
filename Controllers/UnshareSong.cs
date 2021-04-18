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
    [ApiController]
    [Route("/api/[controller]")]
    public class UnshareSong : ControllerBase
    {
        private readonly DbContextApp db;
        public UnshareSong(DbContextApp _db)
        {
            db = _db;
        }
        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken, [FromForm] string uid)
        {
            var accountsManager = new Accounts(db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var elementToUnshare = db.SharedSongs.Single(s => s.uid.Equals(uid));
            if (!elementToUnshare.userId.Equals(user.Id)) return Unauthorized("This share is not yours!");
            db.SharedSongs.Remove(elementToUnshare);
            db.SaveChanges();
            
            return Ok("Unshared successfully");
        }

        [HttpPost]
        [Route("All")]
        public IActionResult DeleteAll([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var allShares = db.SharedSongs.Where(element => element.userId.Equals(user.Id));
            db.SharedSongs.RemoveRange(allShares);
            db.SaveChanges();
            
            return Ok("All shares were deleted successfully");
        }
    }
}
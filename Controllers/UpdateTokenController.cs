/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System.Threading.Tasks;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class UpdateTokenController : ControllerBase
    {
        private readonly DbContextApp db;

        public UpdateTokenController(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromForm] string sessionToken, [FromForm] string token)
        {
            var accountsManager = new Accounts(db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            user.GoogleToken = token;
            db.Users.Update(user);
            await db.SaveChangesAsync();

            return Ok();
        }
    }
}
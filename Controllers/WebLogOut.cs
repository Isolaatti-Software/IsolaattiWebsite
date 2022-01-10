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
    [Route("[controller]")]
    public class WebLogOut : Controller
    {
        private readonly DbContextApp _db;

        public WebLogOut(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var accountsManager = new Accounts(_db);
            accountsManager.RemoveAToken(Request.Cookies["isolaatti_user_session_token"]);
            Response.Cookies.Delete("isolaatti_user_session_token");
            return RedirectToPage("/Index");
        }

        [HttpGet]
        [Route("All")]
        public IActionResult CloseAllSessions()
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return NotFound();
            accountsManager.RemoveAllUsersTokens(user.Id);
            return RedirectToPage("/Index");
        }
    }
}
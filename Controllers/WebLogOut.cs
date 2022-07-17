using System.Threading.Tasks;
using isolaatti_API.Models;
using isolaatti_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    public class WebLogOut : Controller
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public WebLogOut(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await _accounts.RemoveAToken(Request.Cookies["isolaatti_user_session_token"]);
            Response.Cookies.Delete("isolaatti_user_session_token");
            return RedirectToPage("/Index");
        }

        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> CloseAllSessions()
        {
            var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return NotFound();
            await _accounts.RemoveAllUsersTokens(user.Id);
            return RedirectToPage("/Index");
        }
    }
}
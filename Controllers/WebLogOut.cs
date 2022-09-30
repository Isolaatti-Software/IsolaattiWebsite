using System.Threading.Tasks;
using Isolaatti.Classes.Authentication;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [Route("[controller]")]
    public class WebLogOut : Controller
    {
        private readonly IAccounts _accounts;

        public WebLogOut(IAccounts accounts)
        {
            _accounts = accounts;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tokenCookie = Request.Cookies["isolaatti_user_session_token"];
            if (tokenCookie == null)
            {
                return NotFound();
            }
            var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return NotFound();
            
            
            var decodedToken = AuthenticationTokenSerializable.FromString(tokenCookie);
            await _accounts.RemoveAToken(user.Id, decodedToken.Id);
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
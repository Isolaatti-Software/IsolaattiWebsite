using System;
using System.Threading.Tasks;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    public class ExternalSignInController : ControllerBase
    {
        private readonly DbContextApp _db;

        public ExternalSignInController(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        [HttpGet]
        [Route("Web")]
        public async Task<IActionResult> Index([FromQuery] string accessToken, [FromQuery] string then = "")
        {
            var accountManager = new Accounts(_db);
            accountManager.DefineHttpRequestObject(Request);

            // this call won't create an account if one already exists
            await accountManager.MakeAccountFromGoogleAccount(accessToken);

            var sessionToken = await accountManager.CreateTokenForGoogleUser(accessToken);

            // let's save this token
            _db.SessionTokens.Add(sessionToken);
            await _db.SaveChangesAsync();

            // let's put this token on cookies
            Response.Cookies.Append("isolaatti_user_session_token", sessionToken.Token, new CookieOptions()
            {
                Expires = DateTimeOffset.Now.AddMonths(1)
            });
            if (!then.Equals(""))
            {
                return Redirect(then);
            }

            return RedirectToPage("/Index");
        }
    }
}
using System;
using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ExternalSignInController : ControllerBase
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public ExternalSignInController(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        [HttpGet]
        [Route("Web")]
        public async Task<IActionResult> Index([FromQuery] string accessToken, [FromQuery] string then = "")
        {
            // this call won't create an account if one already exists
            await _accounts.MakeAccountFromGoogleAccount(accessToken);

            var sessionToken = await _accounts.CreateTokenForGoogleUser(accessToken);

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

        [HttpGet]
        [Route("ValidateGoogleIdToken")]
        public async Task<IActionResult> ValidateGoogleIdToken([FromHeader] string googleIdToken)
        {
            // this call won't create an account if one already exists
            await _accounts.MakeAccountFromGoogleAccount(googleIdToken);

            var sessionToken = await _accounts.CreateTokenForGoogleUser(googleIdToken);

            // let's save this token
            _db.SessionTokens.Add(sessionToken);
            await _db.SaveChangesAsync();

            return Ok(new Classes.ApiEndpointsResponseDataModels.SessionToken
            {
                Created = DateTime.Now,
                Expires = DateTime.Now.AddMonths(12),
                Token = sessionToken.Token
            });
        }
    }
}
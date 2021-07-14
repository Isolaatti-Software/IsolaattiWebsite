using System;
using System.Linq;
using FirebaseAdmin.Auth;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    public class ValidateGoogleAccessToken : ControllerBase
    {
        private readonly DbContextApp _db;

        public ValidateGoogleAccessToken(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        [HttpPost]
        public IActionResult Index([FromForm] string accessToken)
        {
            var accountManager = new Accounts(_db);
            accountManager.DefineHttpRequestObject(Request);
            
            // this call won't create an account if one already exists
            accountManager.MakeAccountFromGoogleAccount(accessToken);
            
            var sessionToken = accountManager.CreateTokenForGoogleUser(accessToken);
            
            // let's save this token
            _db.SessionTokens.Add(sessionToken);
            _db.SaveChanges();
            
            // let's put this token on cookies
            Response.Cookies.Append("isolaatti_user_session_token",sessionToken.Token, new CookieOptions()
            {
                Expires = DateTimeOffset.Now.AddMonths(1)
            });
            return Ok();
        }
    }
}
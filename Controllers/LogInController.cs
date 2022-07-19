using System;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class LogIn : ControllerBase
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public LogIn(DbContextApp appDbContext, IAccounts accounts)
        {
            _db = appDbContext;
            _accounts = accounts;
        }

        [HttpPost]
        public async Task<IActionResult> Index(SignInFormModel data)
        {
            var user = await _db.Users.FirstOrDefaultAsync(_user => _user.Email.Equals(data.Email));
            if (user == null) return NotFound("User not found");

            var tokenObj = await _accounts.CreateNewToken(user.Id, data.Password);
            if (tokenObj == null) return Unauthorized("Could not get session. Password might be wrong");
            return Ok(new Classes.ApiEndpointsResponseDataModels.SessionToken
            {
                Created = DateTime.Now,
                Expires = DateTime.Now.AddMonths(12),
                Token = tokenObj.Token
            });
        }

        [Route("Verify")]
        [HttpPost]
        public async Task<IActionResult> GetUserData([FromHeader(Name = "sessionToken")] string sessionToken)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null)
                return Unauthorized(new SessionTokenValidated
                {
                    IsValid = false,
                    UserId = -1
                });
            return Ok(new SessionTokenValidated
            {
                IsValid = true,
                UserId = user.Id
            });
        }
    }
}
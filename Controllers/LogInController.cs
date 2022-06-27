/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System;
using System.Threading.Tasks;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.Classes.ApiEndpointsResponseDataModels;
using isolaatti_API.Models;
using isolaatti_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace isolaatti_API.Controllers
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
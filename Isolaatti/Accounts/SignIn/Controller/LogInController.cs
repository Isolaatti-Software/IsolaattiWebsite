using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Config;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Isolaatti.Accounts.SignIn.Controller;

[Route("/api/[controller]")]
[ApiController]
public class LogIn : ControllerBase
{
    private readonly DbContextApp _db;
    private readonly IAccountsService _accounts;
    

    public LogIn(DbContextApp appDbContext, IAccountsService accounts)
    {
        _db = appDbContext;
        _accounts = accounts;
    }

    [HttpPost]
    public async Task<IActionResult> Index(SignInFormModel data, [FromHeader] string apiClientId,
        [FromHeader] string apiClientSecret)
    {
        
        var user = await _db.Users.FirstOrDefaultAsync(_user => _user.Email.Equals(data.Email));
        if (user == null) return NotFound("User not found");

        var tokenObj = await _accounts.CreateNewSession(user.Id, data.Password);
        if (tokenObj == null) return Unauthorized("Could not get session. Password might be wrong");
        return Ok(new SessionToken
        {
            Created = DateTime.Now,
            Expires = DateTime.Now.AddMonths(12),
            Token = tokenObj.ToString(),
            UserId = user.Id
        });
    }

    [Route("Verify")]
    [HttpPost]
    public async Task<IActionResult> GetUserData([FromHeader(Name = "sessionToken")] string sessionToken)
    {
        var user = await _accounts.ValidateSession(SessionDto.FromJson(sessionToken));
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
﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace isolaatti_API.Controllers;

[ApiController]
public class UserLinksController : ControllerBase
{
    private readonly DbContextApp _db;

    public UserLinksController(DbContextApp dbContextApp)
    {
        _db = dbContextApp;
    }

    [HttpPost]
    [Route("/api/UserLinks/Create")]
    public async Task<IActionResult> CreateLink([FromHeader(Name = "sessionToken")] string sessionToken)
    {
        var accountsManager = new Accounts(_db);
        var user = await accountsManager.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        var userProfileLink = new UserProfileLink
        {
            Id = $"{QueryNormalization.ReplaceAccents(user.Name).Replace(" ", ".")}.{user.Id}",
            UserId = user.Id
        };

        _db.UserProfileLinks.Add(userProfileLink);
        await _db.SaveChangesAsync();
        return Ok(userProfileLink);
    }

    [HttpPost]
    [Route("/api/UserLinks/Delete")]
    public async Task<IActionResult> DeleteLink([FromHeader(Name = "sessionToken")] string sessionToken)
    {
        var accountsManager = new Accounts(_db);
        var user = await accountsManager.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        var userProfileLink = _db.UserProfileLinks.Single(upl => upl.UserId == user.Id);
        _db.UserProfileLinks.Remove(userProfileLink);
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpGet]
    [Route("/api/UserLinks/Get/{userId:int}")]
    public async Task<IActionResult> GetUserLink([FromHeader(Name = "sessionToken")] string sessionToken, int userId)
    {
        var accountsManager = new Accounts(_db);
        var user = await accountsManager.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        var requestedUser = await _db.Users.FindAsync(userId);
        if (requestedUser == null) return NotFound(new { error = "Requested user not found" });

        try
        {
            var userProfileLink = await _db.UserProfileLinks.SingleAsync(upl => upl.UserId == userId);
            return Ok(new
            {
                isCustom = true,
                customId = userProfileLink.Id
            });
        }
        catch (InvalidOperationException)
        {
            return Ok(new
            {
                isCustom = false,
                url = $"https://isolaatti.com/perfil/{userId}"
            });
        }
    }

    [HttpPost]
    [Route("/api/UserLinks/ChangeUserLink")]
    public async Task<IActionResult> ChangeUserLink([FromHeader(Name = "sessionToken")] string sessionToken,
        SimpleStringData newId)
    {
        var accountsManager = new Accounts(_db);
        var user = await accountsManager.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        if (!_db.UserProfileLinks.Any(upl => upl.UserId == user.Id))
            return Unauthorized(new { error = "Must create link first" });

        var userProfileLink = await _db.UserProfileLinks.SingleAsync(upl => upl.UserId == user.Id);

        if (userProfileLink.Id.Equals(newId.Data)) return Ok();

        if (string.IsNullOrWhiteSpace(newId.Data))
            return BadRequest(new { error = "New id cannot be null or white space" });

        var regex = new Regex("^([a-zA-Z0-9 _-]+)$");
        if (!regex.IsMatch(newId.Data)) return BadRequest("Validation error: custom id must be alphanumeric.");

        if (await _db.UserProfileLinks.FindAsync(newId.Data) != null)
            return Unauthorized(new { error = "New id is not available" });

        // As changing primary keys is not a good idea, I will first delete the original relation and then add a new one
        _db.UserProfileLinks.Remove(userProfileLink);
        var newUserProfileLink = new UserProfileLink
        {
            Id = newId.Data,
            UserId = user.Id
        };
        await _db.UserProfileLinks.AddAsync(newUserProfileLink);
        await _db.SaveChangesAsync();
        return Ok(newUserProfileLink);
    }

    [HttpGet]
    [Route("/{userIdentifier}")]
    public async Task<IActionResult> RedirectToProfile(string userIdentifier)
    {
        var upl = await _db.UserProfileLinks.FindAsync(userIdentifier);
        if (upl == null) return NotFound();
        return RedirectToPage("/Profile", new
        {
            id = upl.UserId
        });
    }
}
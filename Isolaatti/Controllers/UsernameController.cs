using System.Text;
using System.Threading.Tasks;
using Isolaatti.DTOs;
using Isolaatti.Helpers;
using Isolaatti.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/usernames")]
public class UsernameController : ControllerBase
{
    private readonly DbContextApp _db;

    public UsernameController(DbContextApp dbContextApp)
    {
        _db = dbContextApp;
    }

    [Route("check")]
    [HttpGet]
    public async Task<IActionResult> CheckAvailability([FromQuery] string username)
    {
        if (username.IsNullOrWhiteSpace() || username.Trim().Length is < 3 or > 30)
        {
            return BadRequest("Invalid input, not checking");
        }
        
        var exists = await _db.Users.AnyAsync(u => u.UniqueUsername == username.Trim());
        return Ok(new UsernameAvailabilityCheckDto
        {
            Available = !exists
        });
    }
    
}
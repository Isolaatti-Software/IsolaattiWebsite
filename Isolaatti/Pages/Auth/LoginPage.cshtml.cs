using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Database.EF;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Pages.Auth;

public class LoginPage(DbContextApp db) : PageModel
{
    [BindProperty] public required string Username { get; set; }
    [BindProperty] public required string Password { get; set; }
    [BindProperty] public bool RememberMe { get; set; }
    [BindProperty] public bool IncorrectLogin { get; set; }
    
    public async Task<IActionResult> OnPost([FromQuery(Name = "ReturnUrl")] string? returnUrl)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == Username);
        var passwordHasher = new PasswordHasher<User>();
        
        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, Password) == PasswordVerificationResult.Failed)
        {
            IncorrectLogin = true;
            return Page();
        }
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Surname, user.LastName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.UniqueUsername)
        };
        
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties()
            {
                IsPersistent = RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });
        
        return RedirectToPage(returnUrl ?? "/");
    }
}
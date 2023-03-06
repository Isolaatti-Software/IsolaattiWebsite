using System;
using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Services;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages;

public class ImageViewer : PageModel
{
    private readonly DbContextApp _db;
    private readonly IAccounts _accounts;

    public ImageViewer(DbContextApp dbContextApp, IAccounts accounts)
    {
        _db = dbContextApp;
        _accounts = accounts;
    }

    
    public async Task<IActionResult> OnGet(Guid imageId)
    {
        var token = Request.Cookies["isolaatti_user_session_token"];
        var user = await _accounts.ValidateToken(token);
        if (user == null)
        {
            var protocol = Request.IsHttps ? "https://" : "http://";
            var url = $"{protocol}{Request.HttpContext.Request.Host.Value}";
            url += Request.Path;
            return RedirectToPage("LogIn", new
            {
                then = url
            });
        }

        // here it's know that account is correct. Data binding!
        ViewData["name"] = user.Name;
        ViewData["email"] = user.Email;
        ViewData["userId"] = user.Id;
        ViewData["profilePicUrl"] = user.ProfileImageId == null
            ? null
            : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);
        

        return Page();
    }
}
using System;
using System.Threading.Tasks;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages;

public class ImageViewer : PageModel
{
    private readonly DbContextApp _db;

    public ImageViewer(DbContextApp dbContextApp)
    {
        _db = dbContextApp;
    }

    public ProfileImage Image;

    public async Task<IActionResult> OnGet(Guid imageId)
    {
        var token = Request.Cookies["isolaatti_user_session_token"];
        var accountsManager = new Accounts(_db);
        var user = await accountsManager.ValidateToken(token);
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

        Image = await _db.ProfileImages.FindAsync(imageId);
        if (Image == null) return NotFound();

        return Page();
    }
}
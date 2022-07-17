using System.Linq;
using System.Threading.Tasks;
using isolaatti_API.Models;
using isolaatti_API.Services;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.SettingsPages;

public class FeedSettingsContent : PageModel
{
    private readonly DbContextApp _db;
    private readonly IAccounts _accounts;

    public FeedSettingsContent(DbContextApp dbContextApp, IAccounts accounts)
    {
        _db = dbContextApp;
        _accounts = accounts;
    }

    [BindProperty] public bool ShowOwnPostsOnFeed { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
        if (user == null) return RedirectToPage("LogIn");

        // here it's know that account is correct. Data binding!
        ViewData["name"] = user.Name;
        ViewData["email"] = user.Email;
        ViewData["userId"] = user.Id;
        ViewData["password"] = user.Password;
        ViewData["profilePicUrl"] = user.ProfileImageId == null
            ? null
            : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);

        ShowOwnPostsOnFeed = _db.FollowerRelations.Any(fr => fr.TargetUserId == user.Id && fr.UserId == user.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostShowOwnPostsSet()
    {
        var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
        if (user == null) return RedirectToPage("LogIn");

        if (_db.FollowerRelations.Any(fr => fr.TargetUserId == user.Id && fr.UserId == user.Id) == ShowOwnPostsOnFeed)
        {
            return RedirectToPage("/SettingsPages/FeedSettingsContent");
        }

        if (ShowOwnPostsOnFeed)
        {
            _db.FollowerRelations.Add(new FollowerRelation
            {
                UserId = user.Id,
                TargetUserId = user.Id
            });
        }
        else
        {
            var frToDelete = _db.FollowerRelations
                .Single(fr => fr.UserId == user.Id && fr.TargetUserId == user.Id);

            _db.FollowerRelations.Remove(frToDelete);
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("/SettingsPages/FeedSettingsContent");
    }
}
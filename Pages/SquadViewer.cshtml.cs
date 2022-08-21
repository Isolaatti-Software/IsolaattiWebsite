using System;
using System.Threading.Tasks;
using Isolaatti.Services;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages;

public class SquadViewer : PageModel
{
    private readonly IAccounts _accounts;
    
    public SquadViewer(IAccounts accounts)
    {
        _accounts = accounts;
    }

    public Guid SquadId { get; set; }
    
    public async Task<IActionResult> OnGet(Guid squadId)
    {
        var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
        if (user == null) return RedirectToPage("/LogIn");

        // here it's know that account is correct. Data binding!
        ViewData["name"] = user.Name;
        ViewData["email"] = user.Email;
        ViewData["userId"] = user.Id;
        ViewData["profilePicUrl"] = user.ProfileImageId == null
            ? null
            : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);

        SquadId = squadId;

        return Page();
    }
}
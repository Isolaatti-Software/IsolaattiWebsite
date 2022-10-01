using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages.Likes;

public class ReceivedLikes : PageModel
{
    private readonly IAccounts _accounts;
    private readonly DbContextApp _db;

    public ReceivedLikes(IAccounts accounts, DbContextApp db)
    {
        _accounts = accounts;
        _db = db;
    }

    public async Task<IActionResult> OnGet(int id)
    {
        var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
        if (user == null) return RedirectToPage("/LogIn");

        var profile = await _db.Users.FindAsync(id);
        if (profile == null) return NotFound();

        ViewData["profile_id"] = id;
        ViewData["Title"] = $"Discusiones de {profile.Name} a las que la gente le dio like";
        return Page();
    }
}
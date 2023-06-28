using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.DTOs;
using Isolaatti.Services;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages;

public class LogOutPage : PageModel
{
    private readonly IAccounts _accounts;
    public LogOutPage(IAccounts accounts)
    {
    
        _accounts = accounts;
    }

    public async Task<IActionResult> OnGet()
    {
        var currentSession = await _accounts.CurrentSession();
        if (currentSession != null)
        {
            await _accounts.RemoveSession(currentSession.GetDto());
        }
        
        _accounts.RemoveSessionCookie();
        return RedirectToPage("/LogIn");
    }
}
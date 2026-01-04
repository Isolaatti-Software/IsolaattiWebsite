using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages;

public class LogOutPage : PageModel
{
    private readonly IAccountsService _accounts;
    public LogOutPage(IAccountsService accounts)
    {
    
        _accounts = accounts;
    }

    public async Task<IActionResult> OnGet()
    {
        return SignOut(new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
    }
}
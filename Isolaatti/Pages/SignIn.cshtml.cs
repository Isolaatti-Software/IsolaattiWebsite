using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages;

public class SignIn : PageModel
{
    public IActionResult OnGet()
    {
        return Challenge(new AuthenticationProperties() { RedirectUri = "/"});
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages;

[AllowAnonymous]
public class WelcomePage : PageModel
{
    public void OnGet()
    {
        
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages.Auth;

public class PasswordRecover : PageModel
{
    [BindProperty] public string Email { get; set; }
    
    public void OnGet()
    {
        
    }
}
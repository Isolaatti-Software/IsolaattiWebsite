using Isolaatti.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages
{
    public class Index : IsolaattiPageModel
    {
        public IActionResult OnGet()
        {
            if (HttpContext.User.Identity?.IsAuthenticated == false)
            {
                return RedirectToPage("/WelcomePage");
            }
            return Page();
        }
    }
}
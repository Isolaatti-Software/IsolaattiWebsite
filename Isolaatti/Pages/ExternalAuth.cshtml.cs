using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages
{
    public class ExternalAuthPage : PageModel
    {
        public IActionResult OnGet(string then)
        {
            ViewData["then"] = then;
            return Page();
        }
    }
}
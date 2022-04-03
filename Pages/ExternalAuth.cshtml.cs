using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
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
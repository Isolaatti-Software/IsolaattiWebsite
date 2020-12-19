using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Index : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage("/WebApp/Index");
        }
    }
}
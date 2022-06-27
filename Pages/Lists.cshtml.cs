using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Lists : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            return Page();
        }
    }
}
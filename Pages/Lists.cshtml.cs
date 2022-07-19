using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages
{
    public class Lists : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            return Page();
        }
    }
}
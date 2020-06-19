using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.admin
{
    public class LogOut : PageModel
    {
        public IActionResult OnGet()
        {
            Response.Cookies.Delete("name");
            Response.Cookies.Delete("password");
            return RedirectToPage("LogIn");
        }
    }
}
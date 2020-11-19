using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Ocsp;

namespace isolaatti_API.Pages.admin
{
    
    public class Index : PageModel
    {
        public IActionResult OnGet()
        {
            var name = Request.Cookies["name"];
            var password = Request.Cookies["password"];
            if (name != null && password != null)
            {
                return RedirectToPage("AdminPortal");
            }

            return RedirectToPage("LogIn");
        }
    }
}
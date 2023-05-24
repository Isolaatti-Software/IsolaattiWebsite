using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages.Bootloader;

[IsolaattiAuth]
public class Index : IsolaattiPageModel
{
    public IActionResult OnGet([FromQuery] bool hideNav = false)
    {

        Response.Cookies.Append("isolaatti_hidenav", hideNav ? "yes" : "no");
        
        return RedirectToPage("/Index");
    }
}
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages.Bootloader;

[Authorize]
public class Index : IsolaattiPageModel
{
    public IActionResult OnGet([FromQuery] bool hideNav = false)
    {

        Response.Cookies.Append("isolaatti_hidenav", hideNav ? "yes" : "no");
        
        return RedirectToPage("/Index");
    }
}
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages
{
    [Authorize]
    public class PostEditor : IsolaattiPageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
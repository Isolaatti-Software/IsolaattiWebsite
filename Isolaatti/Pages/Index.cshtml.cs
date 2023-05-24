using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages
{
    [IsolaattiAuth]
    public class Index : IsolaattiPageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
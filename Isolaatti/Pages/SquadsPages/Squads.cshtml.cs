using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages.SquadsPages
{
    [IsolaattiAuth]
    public class Squads : IsolaattiPageModel
    {
        public IActionResult OnGet()
        {
            ViewData["squadsRouter"] = true;
            return Page();
        }
    }
}
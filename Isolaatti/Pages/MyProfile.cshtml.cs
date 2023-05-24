using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages
{
    [IsolaattiAuth]
    public class MyProfile : IsolaattiPageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
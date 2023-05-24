using System;
using System.Threading.Tasks;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages;

[IsolaattiAuth]
public class ImageViewer : IsolaattiPageModel
{
    public async Task<IActionResult> OnGet(Guid imageId)
    {
        return Page();
    }
}
using System;
using System.Threading.Tasks;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/LinkPreview")]
public class LinkPreview : IsolaattiController
{
    [IsolaattiAuth]
    [HttpGet]
    public async Task<IActionResult> Index(string link)
    {
        throw new NotImplementedException();
    }
}
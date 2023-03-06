using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/LinkPreview")]
public class LinkPreview : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(string link)
    {
        throw new NotImplementedException();
    }
}
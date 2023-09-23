using System.Threading.Tasks;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/push_notifications")]
public class PushNotificationsController : IsolaattiController
{
    
    [Route("register_device")]
    [HttpPut]
    [IsolaattiAuth]
    public async Task<IActionResult> RegisterDevice()
    {
        return Ok();
    }
}
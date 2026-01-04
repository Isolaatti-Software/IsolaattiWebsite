using System.Threading.Tasks;
using Isolaatti.RealtimeInteraction.Entity;
using Isolaatti.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.RealtimeInteraction.Controller;

[Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}, {CookieAuthenticationDefaults.AuthenticationScheme}")]
[ApiController]
public class RealtimeController : ControllerBase
{
    private readonly SocketIoServiceKeysRepository _keysRepository;

    public RealtimeController(SocketIoServiceKeysRepository socketIoServiceKeysRepository)
    {
        _keysRepository = socketIoServiceKeysRepository;
    }
    
    [HttpPost]
    [Route("/realtime-service/verify-key")]
    public async Task<IActionResult> Index(SocketIoServiceKey key)
    {
        if (!await _keysRepository.Exists(key))
        {
            return Unauthorized();
        }

        await _keysRepository.RemoveKey(key);
        
        return Ok();
    }
}
using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.RealtimeInteraction.Entity;
using Isolaatti.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.RealtimeInteraction.Controller;

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
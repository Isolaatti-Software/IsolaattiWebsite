using System.Threading.Tasks;
using Isolaatti.Notifications.Dto;
using Isolaatti.Notifications.PushNotifications;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/push_notifications")]
public class PushNotificationsController : IsolaattiController
{
    private readonly RegisterDeviceMessaging _registerDeviceMessaging;
    
    public PushNotificationsController(RegisterDeviceMessaging registerDeviceMessaging)
    {
        _registerDeviceMessaging = registerDeviceMessaging;
    }
    
    [Route("register_device")]
    [HttpPut]
    [IsolaattiAuth]
    public IActionResult RegisterDevice([FromForm(Name = "token")] string token)
    {
        _registerDeviceMessaging.RegisterDevice(new RegisterDeviceMessagingDto
        {
            SessionId = CurrentSessionDto.SessionId,
            Token = token,
            UserId = User.Id
        });
        
        return Ok();
    }
}
using Isolaatti.Notifications.Dto;
using Isolaatti.Notifications.PushNotifications;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Notifications.Controller;

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
    [Authorize]
    public IActionResult RegisterDevice([FromForm(Name = "token")] string token)
    {
        _registerDeviceMessaging.RegisterDevice(new RegisterDeviceMessagingDto
        {
            SessionId = "",
            Token = token,
            UserId = User.Id
        });
        
        return Ok();
    }
}
using Isolaatti.Notifications.Dto;
using Isolaatti.Notifications.PushNotifications;
using Isolaatti.Utils;

using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Notifications.Controller;

[ApiController]
[Route("/api/push_notifications")]
public class PushNotificationsController : ProfileRequiredController
{
    private readonly RegisterDeviceMessaging _registerDeviceMessaging;
    
    public PushNotificationsController(RegisterDeviceMessaging registerDeviceMessaging)
    {
        _registerDeviceMessaging = registerDeviceMessaging;
    }
    
    [Route("register_device")]
    [HttpPut]
    
    public IActionResult RegisterDevice([FromForm(Name = "token")] string token)
    {
        _registerDeviceMessaging.RegisterDevice(new RegisterDeviceMessagingDto
        {
            //SessionId = CurrentSessionDto.SessionId.ToString(),
            Token = token,
            UserId = Profile.Id
        });
        
        return Ok();
    }
}
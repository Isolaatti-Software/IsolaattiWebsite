using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[Route("/api/Notifications")]
[ApiController]
public class NotificationsController : IsolaattiController
{
    private readonly NotificationsService _notifications;
    public NotificationsController(NotificationsService notifications)
    {
        _notifications = notifications;
    }
        
    [IsolaattiAuth]
    [HttpPost]
    [Route("list")]
    public IActionResult GetAll()
    {
        return Ok();
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("DeleteNotification")]
    public IActionResult DeleteANotification(string id)
    {
        return Ok();
    }

    [IsolaattiAuth]
    [Route("Delete/All")]
    [HttpPost]
    public IActionResult DeleteAll()
    {
        return Ok();
    }

    [IsolaattiAuth]
    [Route("MarkAsRead")]
    [HttpPost]
    public IActionResult MarkAsRead()
    {
        return Ok();
    }
}
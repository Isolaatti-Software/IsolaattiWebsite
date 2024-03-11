using Isolaatti.Notifications.Dto;
using Isolaatti.Notifications.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
    [HttpGet]
    [Route("list")]
    public IActionResult GetAll(long? after = null)
    {
        return Ok(new
        {
            result = _notifications.GetUserNotifications(User.Id, after)
        });
    }

    [IsolaattiAuth]
    [HttpDelete]
    [Route("delete_notification")]
    public async Task<IActionResult> DeleteANotification(long id)
    {
        await _notifications.DeleteNotification(User.Id, id);
        return Ok();
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("delete_many")]
    public async Task<IActionResult> DeleteManyNotifications(DeleteManyNotifications deleteManyNotificationsDto)
    {
        await _notifications.DeleteNotification(User.Id, deleteManyNotificationsDto.Ids);
        return Ok();
    }

    [IsolaattiAuth]
    [Route("Delete/All")]
    [HttpPost]
    public async Task<IActionResult> DeleteAll()
    {
        await _notifications.DeleteAllNotifications(User.Id);
        return Ok();
    }

    [IsolaattiAuth]
    [Route("MarkAsRead")]
    [HttpPost]
    public async Task<IActionResult> MarkAsRead(long notificationId)
    {
        await _notifications.MarkAsRead(notificationId, User.Id);
        return Ok();
    }
}
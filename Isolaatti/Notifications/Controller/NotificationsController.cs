using System.Threading.Tasks;
using Isolaatti.Notifications.Dto;
using Isolaatti.Notifications.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Notifications.Controller;

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
    public async Task<IActionResult> GetAll(string after)
    {
        return Ok(await _notifications.GetUserNotifications(User.Id, after));
    }

    [IsolaattiAuth]
    [HttpDelete]
    [Route("delete_notification")]
    public async Task<IActionResult> DeleteANotification(string id)
    {
        await _notifications.DeleteNotification(User.Id, id);
        return Ok();
    }

    [IsolaattiAuth]
    [HttpDelete]
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
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        await _notifications.MarkAsRead(notificationId, User.Id);
        return Ok();
    }
}
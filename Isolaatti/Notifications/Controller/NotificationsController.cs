using System.Threading.Tasks;
using Isolaatti.Notifications.Dto;
using Isolaatti.Notifications.Services;
using Isolaatti.Utils;

using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[Route("/api/Notifications")]
[ApiController]
public class NotificationsController : ProfileRequiredController
{
    private readonly NotificationsService _notifications;
    public NotificationsController(NotificationsService notifications)
    {
        _notifications = notifications;
    }
        
    
    [HttpGet]
    [Route("list")]
    public IActionResult GetAll(long? after = null)
    {
        return Ok(new
        {
            result = _notifications.GetUserNotifications(Profile.Id, after)
        });
    }

    
    [HttpDelete]
    [Route("delete_notification")]
    public async Task<IActionResult> DeleteANotification(long id)
    {
        await _notifications.DeleteNotification(Profile.Id, id);
        return Ok();
    }

    
    [HttpPost]
    [Route("delete_many")]
    public async Task<IActionResult> DeleteManyNotifications(DeleteManyNotifications deleteManyNotificationsDto)
    {
        await _notifications.DeleteNotification(Profile.Id, deleteManyNotificationsDto.Ids);
        return Ok();
    }

    
    [Route("Delete/All")]
    [HttpPost]
    public async Task<IActionResult> DeleteAll()
    {
        await _notifications.DeleteAllNotifications(Profile.Id);
        return Ok();
    }

    
    [Route("MarkAsRead")]
    [HttpPost]
    public async Task<IActionResult> MarkAsRead(long notificationId)
    {
        await _notifications.MarkAsRead(notificationId, Profile.Id);
        return Ok();
    }
}
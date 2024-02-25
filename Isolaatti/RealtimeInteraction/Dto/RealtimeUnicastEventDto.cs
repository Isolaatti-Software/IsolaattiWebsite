using System;
using Isolaatti.Notifications.Entity;

namespace Isolaatti.RealtimeInteraction.Dto;

public class RealtimeUnicastEventDto
{
    public int UserId { get; set; }
    public Guid ClientId { get; set; }
    public NotificationEntity NotificationEntity { get; set; }
}
using Isolaatti.Notifications.Entity;
using System;

namespace Isolaatti.Notifications.Dto;

public class NotificationDto
{
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public int UserId { get; set; }
    public bool Read { get; set; }

    public object Payload { get; set; }

       
    public static NotificationDto FromEntity(Notification entity)
    {
        return new NotificationDto
        {
            Id = entity.Id,
            Date = entity.TimeStamp,
            UserId = entity.UserId,
            Read = entity.Read,
            Payload = entity.Payload
        };
    }
}

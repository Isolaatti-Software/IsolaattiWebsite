using Isolaatti.Notifications.Entity;
using System;

namespace Isolaatti.Notifications.Dto;

public class NotificationDto
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public int UserId { get; set; }
    public bool Read { get; set; }

    public PayloadDto Payload { get; set; }


    public class PayloadDto
    {
        
        public string Type { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string IntentData { get; set; }
        public static PayloadDto FromEntity(NotificationPayloadEntity entity)
        {
            return new PayloadDto()
            {
                Type = entity.Type,
                AuthorId = entity.AuthorId,
                IntentData = entity.IntentData
            };
        }
    }
    public static NotificationDto FromEntity(NotificationEntity entity)
    {
        return new NotificationDto
        {
            Id = entity.Id,
            Date = entity.TimeStamp,
            UserId = entity.UserId,
            Read = entity.Read,
            Payload = PayloadDto.FromEntity(entity.Payload)
        };
    }
}

using Isolaatti.Notifications.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Isolaatti.Notifications.Dto;

public class NotificationDto
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public int UserId { get; set; }
    public bool Read { get; set; }

    public JsonNode? Data { get; set; }

    
    public static NotificationDto FromEntity(NotificationEntity entity)
    {
        return new NotificationDto
        {
            Id = entity.Id,
            Date = entity.TimeStamp,
            UserId = entity.UserId,
            Read = entity.Read,
            Data = JsonSerializer.SerializeToNode(entity.Data)
        };
    }
}

using System;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace Isolaatti.Notifications.Entity
{
    public class NotificationEntity
    {
        public long Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public int UserId { get; set; }
        public NotificationPayloadEntity Payload { get; set; }
        public bool Read { get; set; }

        public NotificationEntity()
        {
            TimeStamp = DateTime.Now.ToUniversalTime();
            Read = false;
        }

        public bool ReSend()
        {
            return DateTime.Now.ToUniversalTime() - TimeStamp > TimeSpan.FromMinutes(10);
        }

        public bool ShouldReinsert(NotificationEntity notificationEntity)
        {
            return true;
        }

       

    }

    
}
using System;
using Isolaatti.Enums;

namespace Isolaatti.Notifications.Entity
{
    public class SocialNotification
    {
        public string Id { get; }
        public DateTime TimeStamp { get; }
        public int UserId { get; set; }
        public NotificationType Type { get; set; }
        public long PostRelated { get; set; }
        public string DataJson { get; set; }
        public bool Read { get; set; }

        public SocialNotification()
        {
            TimeStamp = DateTime.Now.ToUniversalTime();
            Read = false;
        }
    }
}
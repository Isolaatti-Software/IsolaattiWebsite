using System;

namespace Isolaatti.Models.MongoDB
{
    public class SocialNotification
    {
        public string Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public int UserId { get; set; }
        public int Type { get; set; }
        public long PostRelated { get; set; }
        public string DataJson { get; set; }
        public bool Read { get; set; }
    }
}
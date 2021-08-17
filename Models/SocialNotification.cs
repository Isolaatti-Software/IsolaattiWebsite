using System;

namespace isolaatti_API.Models
{
    public class SocialNotification
    {
        public Guid Id { get; set; }
        public DateTime TimeSpan { get; set; }
        public int UserId { get; set; }
        public int Type { get; set; }
        public long PostRelated { get; set; }
        public string DataJson { get; set; }
        public bool Read { get; set; }
    }
}
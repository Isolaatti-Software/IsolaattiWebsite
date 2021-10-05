using System;

namespace isolaatti_API.Models
{
    public class UserSeenPostHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public int TimesSeen { get; set; }
    }
}
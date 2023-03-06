using System;

namespace Isolaatti.Models
{
    public class UserSeenPostHistory
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public long PostId { get; set; }
        public int TimesSeen { get; set; }
    }
}
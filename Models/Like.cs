using System;

namespace isolaatti_API.Models
{
    public class Like
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public Guid TargetUserId { get; set; }
        public DateTime Date { get; set; }
    }
}
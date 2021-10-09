using System;

namespace isolaatti_API.Models
{
    public class PostReport
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public int Category { get; set; }
        public string UserReason { get; set; }
        public bool Viewed { get; set; }
    }
}
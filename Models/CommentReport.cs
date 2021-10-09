using System;

namespace isolaatti_API.Models
{
    public class CommentReport
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public int Category { get; set; }
        public string UserReason { get; set; }
        public bool Viewed { get; set; }
        public DateTime Date { get; set; }
    }
}
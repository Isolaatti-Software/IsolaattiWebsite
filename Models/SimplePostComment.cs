using System;

namespace isolaatti_API.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string TextContent { get; set; }
        public Guid WhoWrote { get; set; }
        public Guid SimpleTextPostId { get; set; }
        public Guid TargetUser { get; set; }
        public int Privacy { get; set; }
        public string AudioUrl { get; set; }
        public DateTime Date { get; set; }
    }
}
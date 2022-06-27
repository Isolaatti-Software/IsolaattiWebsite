using System;

namespace isolaatti_API.Models
{
    public class Comment
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int WhoWrote { get; set; }
        public long SimpleTextPostId { get; set; }
        public int TargetUser { get; set; }
        public int Privacy { get; set; }
        public string AudioUrl { get; set; }
        public Guid? AudioId { get; set; }
        public DateTime Date { get; set; }

        public Comment()
        {
            Date = DateTime.Now.ToUniversalTime();
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Isolaatti.Models
{
    public class Comment
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        
        [ForeignKey("User")]
        public int WhoWrote { get; set; }
        
        public long SimpleTextPostId { get; set; }
        public int TargetUser { get; set; }
        public int Privacy { get; set; }
        public string? AudioId { get; set; }
        public DateTime Date { get; set; }
        
        // Response in the same discussion
        public long? ResponseForCommentId { get; set; }
        
        // Other discussions
        public long? LinkedDiscussionId { get; set; }
        public long? LinkedCommentId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        public Comment()
        {
            Date = DateTime.Now.ToUniversalTime();
        }
    }
}
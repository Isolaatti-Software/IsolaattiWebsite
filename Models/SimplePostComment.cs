using System;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string AudioUrl { get; set; }
        public string? AudioId { get; set; }
        public DateTime Date { get; set; }
        
        public User User { get; set; }

        public Comment()
        {
            Date = DateTime.Now.ToUniversalTime();
        }
    }
}
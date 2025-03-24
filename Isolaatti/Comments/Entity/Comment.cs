using System;

namespace Isolaatti.Comments.Entity
{
    public class Comment
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int UserId { get; set; }
        public long PostId { get; set; }
        public int TargetUser { get; set; }
        public Guid? AudioId { get; set; }
        public DateTime Date { get; set; }
        public Guid? SquadId { get; set; }


        // Response in the same discussion
        public long? ResponseForCommentId { get; set; }

        // Other discussions
        public long? LinkedDiscussionId { get; set; }
        public long? LinkedCommentId { get; set; }

        public bool Modified { get; set; }


        public Comment()
        {
            Date = DateTime.Now.ToUniversalTime();
        }
    }
}
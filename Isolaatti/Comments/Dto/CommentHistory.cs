using System.Collections.Generic;
using Isolaatti.Comments.Entity;

namespace Isolaatti.Comments.Dto
{
    public class CommentHistory
    {
        public long CommentId { get; set; }
        public List<Comment> Comments { get; set; }
    }
}

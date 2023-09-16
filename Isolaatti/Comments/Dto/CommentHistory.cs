using Isolaatti.Comments.Entity;
using System.Collections.Generic;

namespace Isolaatti.Comments.Dto
{
    public class CommentHistory
    {
        public long CommentId { get; set; }
        public List<Comment> Comments { get; set; }
    }
}

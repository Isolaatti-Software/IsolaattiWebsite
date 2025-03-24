using System;

namespace Isolaatti.Comments.Dto
{
    public class MakeCommentModel
    {
        public string Content { get; set; }
        public Guid? AudioId { get; set; }
    }
}
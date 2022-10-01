using System;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels
{
    public class FeedComment
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public long PostId { get; set; }
        public int TargetUserId { get; set; }
        public int Privacy { get; set; }
        public string? AudioId { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool UserIsOwner { get; set; }
    }
}
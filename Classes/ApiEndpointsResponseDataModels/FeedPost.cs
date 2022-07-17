using System;

namespace isolaatti_API.Classes.ApiEndpointsResponseDataModels
{
    public class FeedPost
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public int UserId { get; set; }
        public bool Liked { get; set; }
        public string Content { get; set; }
        public int NumberOfLikes { get; set; }
        public int NumberOfComments { get; set; }
        public int Privacy { get; set; }
        public Guid? AudioId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public bool UserIsOwner { get; set; }
    }
}
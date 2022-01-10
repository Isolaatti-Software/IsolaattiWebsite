using System.Collections.Generic;

namespace isolaatti_API.Classes.ApiEndpointsResponseDataModels
{
    public class FeedResponse
    {
        public long LastPostId { get; set; }
        public bool MoreContent { get; set; }
        public List<FeedPost> Posts { get; set; }
    }
}
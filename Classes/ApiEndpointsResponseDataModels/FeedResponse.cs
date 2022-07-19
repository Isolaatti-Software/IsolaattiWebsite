using System.Collections.Generic;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels
{
    public class FeedResponse
    {
        public long LastPostId { get; set; }
        public bool MoreContent { get; set; }
        public List<FeedPost> Posts { get; set; }
    }
}
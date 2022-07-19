using System.Collections.Generic;

namespace Isolaatti.Classes.NotificationsData
{
    public class LikeData
    {
        public long PostId { get; set; }
        public List<int> AuthorsIds { get; set; }
        public long NumberOfLikes { get; set; }
    }
}
using System.Collections.Generic;

namespace isolaatti_API.Classes.NotificationsData
{
    public class PostData
    {
        public long PostId { get; set; }
        public List<int> AuthorsIds { get; set; }
        public long NumberOfComments { get; set; }
    }
}
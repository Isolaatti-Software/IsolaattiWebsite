using System;
using System.Collections.Generic;

namespace isolaatti_API.Classes.NotificationsData
{
    public class LikeData
    {
        public Guid PostId { get; set; }
        public List<Guid> AuthorsIds {get; set; }
        public long NumberOfLikes { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace isolaatti_API.Classes.NotificationsData
{
    public class PostData
    {
        public Guid PostId { get; set; }
        public List<Guid> AuthorsIds { get; set; }
        public long NumberOfComments { get; set; }
    }
}
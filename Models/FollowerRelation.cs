using System;

namespace isolaatti_API.Models
{
    public class FollowerRelation
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public int TargetUserId { get; set; }
    }
}
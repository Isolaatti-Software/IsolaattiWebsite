using System;
using Isolaatti.Accounts.Data.Entity;

namespace Isolaatti.Models
{
    public class FollowerRelation
    {
        public Guid Id { get; set; }
        public int ProfileId { get; set; }
        public int TargetProfileId { get; set; }
        
        public Profile Profile { get; set; }
        public Profile TargetProfile { get; set; }
    }
}
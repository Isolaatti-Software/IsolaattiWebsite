using System;
using System.ComponentModel.DataAnnotations.Schema;
using Isolaatti.Accounts.Data.Entity;

namespace Isolaatti.Models
{
    public class FollowerRelation
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public int TargetUserId { get; set; }
        
        public User User { get; set; }
        public User TargetUser { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Isolaatti.Models
{
    public class Like
    {
        // public long Id { get; set; }
        public Guid LikeId { get; set; }
        
        public long PostId { get; set; }
        public int UserId { get; set; }
        public int TargetUserId { get; set; }
        public DateTime Date { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }
        

        public Like()
        {
            Date = DateTime.Now.ToUniversalTime();
        }
    }
}
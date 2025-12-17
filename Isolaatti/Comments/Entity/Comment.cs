using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Posting.Data;

namespace Isolaatti.Comments.Entity
{
    public class Comment
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        
        [JsonIgnore]
        public virtual User User { get; set; }
        public int UserId { get; set; }
        public long PostId { get; set; }
        
        [JsonIgnore]
        public Post Post { get; set; }
        
        [JsonIgnore]
        public User TargetUser { get; set; }
        public int TargetUserId { get; set; }
        public DateTime Date { get; set; }
        public Guid? SquadId { get; set; }


        // Response in the same discussion
        public long? ResponseForCommentId { get; set; }

        [JsonIgnore]
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
        
        [JsonIgnore]
        public Comment? ParentComment { get; set; }

        public bool Modified { get; set; }
        
        [NotMapped]
        public bool IsFromUser { get; set; }


        public Comment()
        {
            Date = DateTime.Now.ToUniversalTime();
        }
    }
}
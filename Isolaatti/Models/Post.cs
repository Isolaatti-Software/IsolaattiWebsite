using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using Isolaatti.Comments.Entity;

namespace Isolaatti.Models
{
    public class Post
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int UserId { get; set; }
        public int Privacy { get; set; }
        public DateTimeOffset Date { get; set; }
        public string? AudioId { get; set; }
        public Guid? SquadId { get; set; }
        public long? LinkedDiscussionId { get; set; }
        public long? LinkedCommentId { get; set; }
        
        [JsonIgnore]
        public User User { get; set; }
        [JsonIgnore]
        public virtual ICollection<Like> Likes { get; set; }
        [JsonIgnore]
        public virtual ICollection<Comment> Comments { get; set; }
        [JsonIgnore]
        public virtual Squad Squad { get; set; }

        public Post()
        {
            Date = DateTime.Now.ToUniversalTime();
        }
    }
}
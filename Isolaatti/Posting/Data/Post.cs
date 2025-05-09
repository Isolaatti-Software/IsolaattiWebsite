using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Comments.Entity;
using Isolaatti.Images.Data;
using Isolaatti.Likes.Data;
using Isolaatti.Squads.Data;

namespace Isolaatti.Posting.Data
{
    public class Post
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int UserId { get; set; }
        public int Privacy { get; set; }
        public DateTimeOffset Date { get; set; }
        public Guid? SquadId { get; set; }
        
        [JsonIgnore]
        public string? AudioStoragePath { get; set; }
        public long AudioDurationMs { get; set; }
        
        [NotMapped]
        public bool HasAudio => AudioStoragePath != null;
        
        [JsonIgnore]
        public User User { get; set; }
        [JsonIgnore]
        public virtual ICollection<Like> Likes { get; set; }
        [JsonIgnore]
        public virtual ICollection<Comment> Comments { get; set; }
        [JsonIgnore]
        public virtual Squad? Squad { get; set; }
        public virtual ICollection<Image> Images { get; } = new List<Image>();
        public bool IsDraft { get; set; }

        public Post()
        {
            Date = DateTime.Now.ToUniversalTime();
        }
    }
}
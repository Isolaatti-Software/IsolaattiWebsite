using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace Isolaatti.Models
{
    public class Post
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int UserId { get; set; }
        public int Privacy { get; set; }
        public DateTime Date { get; set; }
        public string? AudioId { get; set; }
        public Guid? SquadId { get; set; }
        public long? LinkedDiscussionId { get; set; }
        public long? LinkedCommentId { get; set; }

        [NotMapped] public int NumberOfLikes { get; set; }
        [NotMapped] public int NumberOfComments { get; set; }
        [NotMapped] public string UserName { get; set; }
        [NotMapped] public string SquadName { get; set; }
        [NotMapped] public bool Liked { get; set; }

        public Post()
        {
            Date = DateTime.Now.ToUniversalTime();
        }

        public Post SetLiked(bool liked)
        {
            Liked = liked;
            return this;
        }

        public Post SetNumberOfLikes(int number)
        {
            NumberOfLikes = number;
            return this;
        }

        public Post SetNumberOfComments(int number)
        {
            NumberOfComments = number;
            return this;
        }

        public Post SetUserName(string name)
        {
            UserName = name;
            return this;
        }

        public Post SetSquadName(string name)
        {
            SquadName = name;
            return this;
        }
    }
}
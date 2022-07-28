using System;
using System.Text.Json.Serialization;

namespace Isolaatti.Models
{
    public class SimpleTextPost
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int UserId { get; set; }
        public int NumberOfLikes { get; set; }
        public int NumberOfComments { get; set; }
        public int Privacy { get; set; }
        public string AudioAttachedUrl { get; set; }
        public string ThemeJson { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public Guid? AudioId { get; set; }
        public Guid? SquadId { get; set; }

        [JsonIgnore] public User User { get; set; }

        public SimpleTextPost()
        {
            Date = DateTime.Now.ToUniversalTime();
        }
    }
}
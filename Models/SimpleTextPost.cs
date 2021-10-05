using System;

namespace isolaatti_API.Models
{
    public class SimpleTextPost
    {
        public Guid Id { get; set; }
        public string TextContent { get; set; }
        public Guid UserId { get; set; }
        public long NumberOfLikes { get; set; }
        public long NumberOfComments { get; set; }
        public int Privacy { get; set; }
        public string AudioAttachedUrl { get; set; }
        public string ThemeJson { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
    }
}
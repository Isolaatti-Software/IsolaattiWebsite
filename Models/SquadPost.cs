using System;

namespace isolaatti_API.Models
{
    public class SquadPost
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int UserId { get; set; }
        public int NumberOfLikes { get; set; }
        public int NumberOfComments { get; set; }
        public string AudioAttachedUrl { get; set; }
        public string ThemeJson { get; set; }
        public DateTime Date { get; set; }
    }
}
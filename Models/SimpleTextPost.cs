namespace isolaatti_API.Models
{
    public class SimpleTextPost
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int UserId { get; set; }
        public long NumberOfLikes { get; set; }
        public int Privacy { get; set; }
    }
}
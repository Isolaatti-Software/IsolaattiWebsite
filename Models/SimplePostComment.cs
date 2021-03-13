namespace isolaatti_API.Models
{
    public class Comment
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int WhoWrote { get; set; }
        public int SimpleTextPostId { get; set; }
        public int Privacy { get; set; }
    }
}
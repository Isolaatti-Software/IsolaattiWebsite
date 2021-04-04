namespace isolaatti_API.Models
{
    public class Comment
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int WhoWrote { get; set; }
        public long SimpleTextPostId { get; set; }
        public int Privacy { get; set; }
        public string AudioUrl { get; set; }
    }
}
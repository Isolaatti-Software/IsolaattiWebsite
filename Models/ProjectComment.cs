namespace isolaatti_API.Models
{
    public class ProjectComment
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int WhoWroteId { get; set; }
        public int ProjectInt { get; set; }
    }
}
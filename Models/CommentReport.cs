namespace isolaatti_API.Models
{
    public class CommentReport
    {
        public int Id { get; set; }
        public long CommentId { get; set; }
        public int Category { get; set; }
        public string UserReason { get; set; }
        public bool Viewed { get; set; }
    }
}
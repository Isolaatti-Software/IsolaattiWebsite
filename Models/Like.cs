namespace isolaatti_API.Models
{
    public class Like
    {
        public int Id { get; set; }
        public long PostId { get; set; }
        public int UserId { get; set; }
        public int TargetUserId { get; set; }
    }
}
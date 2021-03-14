namespace isolaatti_API.Models
{
    public class UserSeenPostHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public long PostId { get; set; }
        public int TimesSeen { get; set; }
    }
}
namespace isolaatti_API.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SongId { get; set; }
        public string SongName { get; set; }
        public string ArtistName { get; set; }
        public bool Seen { get; set; }
        public int type { get; set; }
    }
}

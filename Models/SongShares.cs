using Microsoft.AspNetCore.Mvc;


namespace isolaatti_API.Models
{
    public class SongShares
    {
        public int Id { get; set; }
        public int SharedSongId { get; set; }
        public string uid { get; set; }
        public int userId { get; set; }
    }
}
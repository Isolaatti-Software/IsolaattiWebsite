using System;

namespace isolaatti_API.Models
{
    public class SongQueue
    {
        public int Id { get; set; }
        public string AudioSourceUrl { get; set; }
        public bool Reserved { get; set; }
        public string UserId { get; set; }
        public string SongName { get; set; }
        public string SongArtist { get; set; }
    }
}
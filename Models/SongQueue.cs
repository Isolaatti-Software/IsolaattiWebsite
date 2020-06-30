using System;

namespace isolaatti_API.Models
{
    public class SongQueue
    {
        public int Id { get; set; }
        public string AudioSourceUrl { get; set; }
        public bool Reserved { get; set; }
        public int SongId { get; set; }
        public DateTime ReservationTime { get; set; }
    }
}
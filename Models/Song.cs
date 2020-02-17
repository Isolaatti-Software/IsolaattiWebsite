using Microsoft.Extensions.Hosting.Internal;

namespace isolaatti_API.Models
{
    public class Song
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string OriginalFileName { get; set; }
        public string BassUrl { get; set; }
        public string DrumsUrl { get; set; }
        public string VoiceUrl { get; set; }
        public string OtherUrl { get; set; }
    }
}
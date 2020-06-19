namespace isolaatti_API.Classes
{
    // data to return song's info
    public class SharedSong
    {
        public int OwnerId { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string BassUrl { get; set; }
        public string DrumsUrl { get; set; }
        public string VoiceUrl { get; set; }
        public string OtherUrl { get; set; }
    }
}
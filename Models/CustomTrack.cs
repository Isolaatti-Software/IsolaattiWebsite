namespace isolaatti_API.Models
{
    public class CustomTrack
    {
        public int Id { get; set; }
        public int SongId { get; set; }
        public string DownloadUrl { get; set; }
        public string Name { get; set; }
        public string EffectsAndPropertiesDefinitionJsonObject { get; set; }
    }
}
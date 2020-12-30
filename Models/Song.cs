/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
namespace isolaatti_API.Models
{
    public class Song
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string OriginalFileName { get; set; }
        public string Artist { get; set; }
        public string BassUrl { get; set; }
        public string DrumsUrl { get; set; }
        public string VoiceUrl { get; set; }
        public string OtherUrl { get; set; }
        public bool IsBeingProcessed { get; set; }
        public string Uid { get; set; }
        
    }
}
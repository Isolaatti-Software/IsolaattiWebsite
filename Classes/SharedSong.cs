/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
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
/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
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
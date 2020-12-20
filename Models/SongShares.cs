/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
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
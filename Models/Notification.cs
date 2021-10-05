/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System;

namespace isolaatti_API.Models
{
    public class otification
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public int SongId { get; set; }
        public string SongName { get; set; }
        public string ArtistName { get; set; }
        public bool Seen { get; set; }
        public int type { get; set; }
    }
}

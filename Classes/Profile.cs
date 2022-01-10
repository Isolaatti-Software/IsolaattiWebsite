/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

namespace isolaatti_API.Classes
{
    public class Profile
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string AudioUrl { get; set; }
        public int NumberOfPosts { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}
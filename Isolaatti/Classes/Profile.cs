/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System;

namespace Isolaatti.Classes
{
    public class Profile
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public int NumberOfPosts { get; set; }
        public string ProfileAudioId { get; set; }
        public Guid? ProfilePictureId { get; set; }
        public int NumberOfFollowers { get; set; }
        public int NumberOfFollowings { get; set; }
        public int NumberOfLikes { get; set; }
        public int NumberOfLikesGiven { get; set; }
        public bool IsUserItself { get; set; }
        public bool FollowingThisUser { get; set; }
        public bool ThisUserIsFollowingMe { get; set; }
    }
}
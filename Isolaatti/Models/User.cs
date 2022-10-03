/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace Isolaatti.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        [JsonIgnore] 
        public string Password { get; set; }
        [JsonIgnore] 
        public bool EmailValidated { get; set; }
        [JsonIgnore] 
        public bool ShowEmail { get; set; }
        [NotMapped] 
        public int NumberOfFollowers { get; set; }
        [NotMapped] 
        public int NumberOfFollowing { get; set; }
        [NotMapped]
        public int NumberOfLikes { get; set; }
        [NotMapped] 
        public int NumberOfPosts { get; set; }
        [NotMapped]
        public bool IsUserItself { get; set; }
        [NotMapped]
        public bool FollowingThisUser { get; set; }
        [NotMapped]
        public bool ThisUserIsFollowingMe { get; set; }
        public Guid? ProfileImageId { get; set; }
        public string DescriptionText { get; set; }
        public string DescriptionAudioId { get; set; }

    }
}
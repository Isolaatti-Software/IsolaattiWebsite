/*
 * Isolaatti project
 * Erik Cavazos, 2020
 * This program is not allowed to be copied or reused without explicit permission.
 * erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
 */

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Isolaatti.Accounts.Data.Entity
{
    public class User
    {
        public int Id { get; set; }
        public string UniqueUsername { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        [JsonIgnore] 
        public string Password { get; set; }
        
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
        
        // ProfileImageId is used as first option, and then try ProfileImageUrl
        public string? ProfileImageId { get; set; }
        // ProfileImageUrl is used to save Firebase account image url. It can come from Google, Facebook, etc
        public string? ProfileImageUrl { get; set; }
        public string? DescriptionText { get; set; }
        public string? DescriptionAudioId { get; set; }
        public string? Genre { get; set; }
        public string? CountryCode { get; set; }
        public DateTime? Birthday { get; set; }

    }
}
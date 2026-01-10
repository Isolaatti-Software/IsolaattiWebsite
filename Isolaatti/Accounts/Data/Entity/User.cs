/*
 * Isolaatti project
 * Erik Cavazos, 2020
 * This program is not allowed to be copied or reused without explicit permission.
 * erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Isolaatti.Comments.Entity;
using Isolaatti.Likes.Data;

namespace Isolaatti.Accounts.Data.Entity
{
    public class User
    {

        public static readonly User AnonymousUser = new()
        {
            Id = -1,
            Sub = "anonymous",
            UniqueUsername = "anonymous",
            Name = "Anonymous",
            LastName = "User",
        };
        
        public int Id { get; set; }
        public string Sub { get; set; }
        public string UniqueUsername { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        
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
        public Guid? ProfileImageId { get; set; }
        // ProfileImageUrl is used to save Firebase account image url. It can come from Google, Facebook, etc
        public string? ProfileImageUrl { get; set; }
        public string? DescriptionText { get; set; }
        
        [JsonIgnore]
        public string? AudioStoragePath { get; set; }
        public long AudioDurationMs { get; set; }
        public bool HasAudio => AudioStoragePath != null;
        public string? Genre { get; set; }
        public string? CountryCode { get; set; }
        public DateTime? Birthday { get; set; }

        [JsonIgnore]
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        [JsonIgnore]
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        [JsonIgnore]
        public virtual ICollection<Comment> CommentsReceived { get; set; } = new List<Comment>();

    }
}
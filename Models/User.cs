/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System;
using System.Collections.Generic;

namespace Isolaatti.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool EmailValidated { get; set; }


        // fields for user preferences
        public string UserPreferencesJson { get; set; }
        public bool ShowEmail { get; set; }

        // here store people (followers and following)
        public int NumberOfFollowers { get; set; }
        public int NumberOfFollowing { get; set; }

        public Guid? ProfileImageId { get; set; }
        public string DescriptionText { get; set; }
        public string DescriptionAudioId { get; set; }

        public virtual ICollection<SimpleTextPost> Posts { get; set; }
    }
}
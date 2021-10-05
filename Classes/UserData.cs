/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using isolaatti_API.Models;

namespace isolaatti_API.Classes
{
    public class UserData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DescriptionAudioUrl { get; set; }
        public string Email { get; set; }
    }
}

using System;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Identity;

namespace isolaatti_API.Models
{
    public class SquadModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public string SettingsJson { get; set; }

        // These two attributes are needed to allow join links
        // example. host/JoinSquad/<guid>/IdForHashing/NotHashedKey
        // key is a password that is hashed and then stored in DB
        // IdForHashing is another id apart from primary key, to allow hasher to work
        public string HashedKey { get; set; }
        public string IdForHashing { get; set; }

        public SquadModel()
        {
            IdForHashing = RandomData.GenerateRandomKey(64);
            var key = RandomData.GenerateRandomKey(128);
            var passwordHasher = new PasswordHasher<string>();

            HashedKey = passwordHasher.HashPassword(IdForHashing, key);
        }
    }
}
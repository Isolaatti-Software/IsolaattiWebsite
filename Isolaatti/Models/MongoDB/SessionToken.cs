using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Isolaatti.Models.MongoDB
{
    public class AuthToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [Required]
        public string Guid { get; set; }
        
        [Required]
        public string HashedKey { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string IpAddress { get; set; }
        
        [Required]
        public string UserAgent { get; set; }
        
        [Required]
        public DateTime CreationDate { get; set; }
    }
}
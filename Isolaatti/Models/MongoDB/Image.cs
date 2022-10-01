using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Isolaatti.Models.MongoDB;

public class Image
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [Required]
    public string Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    public Guid SquadId { get; set; }
    
    [Required]
    public string FirebaseObjectPath { get; set; }
}
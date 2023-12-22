using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

    [Required]
    public string Name { get; set; }
    
    public Guid? SquadId { get; set; }
    
    [Required]
    public string IdOnFirebase { get; set; }
    
    public bool Outstanding { get; set; }
    
    [BsonIgnore]
    public string? Username { get; set; }
}
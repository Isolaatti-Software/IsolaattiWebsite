using System.ComponentModel.DataAnnotations;
using Isolaatti.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Isolaatti.Models;

public class SocketIoServiceKey
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [Required(ErrorMessage = "Key is required")]
    public string Key { get; set; }
}
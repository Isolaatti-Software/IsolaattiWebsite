using System;
using Isolaatti.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Isolaatti.Models.MongoDB;

public class SquadInvitation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public Guid SquadId { get; set; }
    public int SenderUserId { get; set; }
    public int RecipientUserId { get; set; }
    public string Message { get; set; }
    public string ResponseMessage { get; set; }
    public DateTime CreationDate { get; set; }
    public SquadInvitationStatus InvitationStatus { get; set; }
}
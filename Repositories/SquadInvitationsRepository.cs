using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Enums;
using Isolaatti.Models.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Isolaatti.Repositories;

public class SquadInvitationsRepository
{
    private readonly IMongoCollection<SquadInvitation> _invitations;
    private readonly MongoDatabaseConfiguration _settings;

    public SquadInvitationsRepository(IOptions<MongoDatabaseConfiguration> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        var database = client.GetDatabase(_settings.DatabaseName);
        _invitations = database.GetCollection<SquadInvitation>(_settings.SquadsInvitationsCollectionName);
    }

    /// <summary>
    /// Method <c>CreateInvitation</c> creates an invitation for a squad
    /// <param name="squadId">The id of the Squad to create the invitation</param>
    /// <param name="senderUserId">The id of the user that wants to create the invitation</param>
    /// <param name="recipientUserId">The id of the user that is invited</param>
    /// <param name="message">A message to convince the recipient user to join the Squad</param>
    /// </summary>
    public async Task CreateInvitation(Guid squadId, int senderUserId, int recipientUserId, string message = null)
    {
        await _invitations.InsertOneAsync(new SquadInvitation
        {
            SquadId = squadId,
            SenderUserId = senderUserId,
            RecipientUserId = recipientUserId,
            Message = message,
            InvitationStatus = SquadInvitationStatus.Requested,
            CreationDate = DateTime.Now.ToUniversalTime()
        });
    }

    public async Task CreateInvitations(Guid squadId, int senderUserId, IEnumerable<int> recipientUserIds, string message = null)
    {
        var invitations = recipientUserIds.Select(userId => new SquadInvitation
        {
            SquadId = squadId,
            SenderUserId = senderUserId,
            RecipientUserId = userId,
            Message = message,
            InvitationStatus = SquadInvitationStatus.Requested,
            CreationDate = DateTime.Now.ToUniversalTime()
        });

        await _invitations.InsertManyAsync(invitations);
    }

    public async Task RemoveInvitation(string id)
    {
        await _invitations.DeleteOneAsync(inv => inv.Id.Equals(id));
    }

    public async Task RemoveInvitationsForASquad(Guid id)
    {
        await _invitations.DeleteManyAsync(inv => inv.SquadId.Equals(id));
    }
    
    public void UpdateInvitationStatus(string id, SquadInvitationStatus status, string responseMessage)
    {
        _invitations
            .UpdateOne(inv => 
                inv.Id.Equals(id),Builders<SquadInvitation>
                .Update
                .Set("InvitationStatus",status)
                .Set("ResponseMessage",responseMessage));
    }

    
    public async Task<bool> SameInvitationExists(Guid squadId, int senderUserId, int recipientUserId)
    {
        return await _invitations.Find(inv =>
            inv.SquadId.Equals(squadId) && inv.SenderUserId.Equals(senderUserId) &&
            inv.RecipientUserId.Equals(recipientUserId))
            .Limit(1)
            .CountDocumentsAsync() > 0;
    }

    public SquadInvitation GetInvitation(string id)
    {
        return _invitations.Find(inv => inv.Id.Equals(id)).Limit(1).FirstOrDefault();
    }

    // Returns the invitations that the people have sent to the user
    public async Task<IEnumerable<SquadInvitation>> GetInvitationsForUser(int userId, string lastId = null)
    {
        if (lastId == null)
        {
            return await _invitations
                .Find(inv => 
                    inv.RecipientUserId.Equals(userId))
                .Limit(20)
                .ToListAsync();
        }
        
        return await _invitations
            .Find(inv => 
                inv.RecipientUserId.Equals(userId) && new ObjectId(inv.Id) > new ObjectId(lastId))
            .Limit(20)
            .ToListAsync();
    }

    // Returns the invitations that the user has sent
    public async Task<IEnumerable<SquadInvitation>> GetInvitationsOfUser(int userId, string lastId = null)
    {
        if (lastId == null)
            return await _invitations
                .Find(inv => 
                    inv.SenderUserId.Equals(userId))
                .Limit(20)
                .ToListAsync();
        
        return await _invitations
            .Find(inv => 
                inv.SenderUserId.Equals(userId) && new ObjectId(inv.Id) > new ObjectId(lastId))
            .Limit(20)
            .ToListAsync();
    }
}
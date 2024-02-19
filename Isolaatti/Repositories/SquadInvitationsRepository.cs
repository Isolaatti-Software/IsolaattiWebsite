using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Enums;
using Isolaatti.Models.MongoDB;
using MongoDB.Driver;

namespace Isolaatti.Repositories;

public class SquadInvitationsRepository
{
    private readonly IMongoCollection<SquadInvitation> _invitations;

    public SquadInvitationsRepository(MongoDatabase mongoDatabase)
    {
        
        _invitations = mongoDatabase.GetSquadInvitationsCollection();
    }

    /// <summary>
    /// Method <c>CreateInvitation</c> creates an invitation for a squad
    /// <param name="squadId">The id of the Squad to create the invitation</param>
    /// <param name="senderUserId">The id of the user that wants to create the invitation</param>
    /// <param name="recipientUserId">The id of the user that is invited</param>
    /// <param name="message">A message to convince the recipient user to join the Squad</param>
    /// </summary>
    public async Task CreateInvitation(Guid squadId, int senderUserId, int recipientUserId, string? message = null)
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

    public async Task CreateInvitations(Guid squadId, int senderUserId, IEnumerable<int> recipientUserIds, string? message = null)
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

    /// <summary>
    /// Updates the invitation
    /// </summary>
    /// <param name="invitationId">Id of the invitation to update.</param>
    /// <param name="message">It is the message that the maker of the invitation writes.</param>
    public void UpdateInvitationMessage(string invitationId, string? message)
    {
        _invitations.UpdateOne(inv => inv.Id.Equals(invitationId), Builders<SquadInvitation>.Update.Set("Message", message));
    }

    
    public async Task<bool> SameInvitationExists(Guid squadId, int senderUserId, int recipientUserId)
    {
        return await _invitations.Find(inv =>
            inv.SquadId.Equals(squadId) && inv.SenderUserId.Equals(senderUserId) &&
            inv.RecipientUserId.Equals(recipientUserId))
            .Limit(1)
            .CountDocumentsAsync() > 0;
    }

    public async Task<SquadInvitation?> GetInvitation(string id)
    {
        return await _invitations.Find(inv => inv.Id.Equals(id)).Limit(1).FirstOrDefaultAsync();
    }

    // Returns the invitations that the people have sent to the user
    public async Task<IEnumerable<SquadInvitation>> GetInvitationsForUser(int userId, string? lastId = null)
    {
        if (lastId == null)
        {
            return await _invitations
                .Find(inv => 
                    inv.RecipientUserId.Equals(userId))
                .Limit(20)
                .ToListAsync();
        }
        
        var paginationFilter = Builders<SquadInvitation>.Filter.Gt("id", lastId);
        var userFilter = Builders<SquadInvitation>.Filter.Eq("RecipientUserId", userId);
        
        return await _invitations
            .Find(paginationFilter & userFilter)
            .Limit(20)
            .ToListAsync();
    }

    // Returns the invitations that the user has sent
    public async Task<IEnumerable<SquadInvitation>> GetInvitationsOfUser(int userId, string? lastId = null)
    {
        if (lastId == null)
            return await _invitations
                .Find(inv => 
                    inv.SenderUserId.Equals(userId))
                .Limit(20)
                .ToListAsync();

        var paginationFilter = Builders<SquadInvitation>.Filter.Gt("id", lastId);
        var userFilter = Builders<SquadInvitation>.Filter.Eq("SenderUserId", userId);
        return await _invitations
            .Find(userFilter & paginationFilter)
            .Limit(20)
            .ToListAsync();
    }
    
    // Return the invitation that matches the criteria
    public SquadInvitation? SearchInvitation(int userId, Guid squadId)
    {
        return _invitations.Find(inv => inv.SquadId.Equals(squadId) && inv.RecipientUserId.Equals(userId)).FirstOrDefault();
    }

    public async Task<IEnumerable<SquadInvitation>> GetInvitationsOfSquad(Guid squadId, string? lastId = null)
    {
        if (lastId == null)
        {
            return await _invitations.Find(inv => inv.SquadId.Equals(squadId))
                .Limit(20)
                .ToListAsync();
        }

        var paginationFilter = Builders<SquadInvitation>.Filter.Gt("id", lastId);
        var squadFilter = Builders<SquadInvitation>.Filter.Eq("SquadId", squadId);
        return await _invitations.Find(paginationFilter & squadFilter)
            .Limit(20)
            .ToListAsync();
    }

    public async Task<long> GetUnseenInvitationsForUser(int userId)
    {
        return await _invitations.Find(inv => inv.RecipientUserId == userId && !inv.Seen).CountDocumentsAsync();
    }

    public async Task RemoveInvitationsFromAndToUser(int userId)
    {
        var filterFrom = Builders<SquadInvitation>.Filter.Eq(si => si.SenderUserId, userId);
        var filterTo = Builders<SquadInvitation>.Filter.Eq(si => si.RecipientUserId, userId);

        await _invitations.DeleteManyAsync(filterFrom | filterTo);
        
    }
}
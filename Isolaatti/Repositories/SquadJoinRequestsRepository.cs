using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Enums;
using Isolaatti.Models.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Isolaatti.Repositories;

public class SquadJoinRequestsRepository
{
    private readonly IMongoCollection<SquadJoinRequest> _joinRequests;
    private readonly SquadsRepository _squads;
    
    public SquadJoinRequestsRepository(MongoDatabase mongoDatabase, SquadsRepository squads)
    {
        _joinRequests = mongoDatabase.GetSquadJoinRequestsCollection();
        _squads = squads;
    }

    public async Task CreateJoinRequest(Guid squadId, int senderUserId, string message)
    {
        await _joinRequests.InsertOneAsync(new SquadJoinRequest
        {
            SquadId = squadId,
            SenderUserId = senderUserId,
            Message = message,
            JoinRequestStatus = SquadInvitationStatus.Requested,
            CreationDate = DateTime.Now.ToUniversalTime()
        });
    }

    public async Task RemoveJoinRequest(string id)
    {
        await _joinRequests.DeleteOneAsync(joinReq => joinReq.Id.Equals(id));
    }

    public bool UpdateJoinRequest(string id, SquadInvitationStatus status, string message)
    {
        var result = _joinRequests
            .UpdateOne(joinReq => joinReq.Id.Equals(id), Builders<SquadJoinRequest>
                .Update
                    .Set("JoinRequestStatus", status)
                    .Set("ResponseMessage", message));

        return result.IsAcknowledged;
    }

    public async Task<bool> SameJoinRequestExists(Guid squadId, int senderUserId)
    {
        return await _joinRequests.Find(joinReq => 
                joinReq.SenderUserId.Equals(senderUserId) && joinReq.SquadId.Equals(squadId))
            .Limit(1)
            .CountDocumentsAsync() > 0;
    }

    public SquadJoinRequest GetJoinRequest(string id)
    {
        return _joinRequests.Find(joinReq => joinReq.Id.Equals(id)).Limit(1).FirstOrDefault();
    }

    /// <summary>
    /// Returns the join requests a given user has sent.
    /// Returns a up to 20 item enumerable. To get the next page pass the last id.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="lastId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<SquadJoinRequest>> GetJoinRequestsOfUser(int userId, string lastId = null)
    {
        if (lastId == null)
            return await _joinRequests
                .Find(joinReq => joinReq.SenderUserId.Equals(userId))
                .Limit(20)
                .ToListAsync();
        
        var paginationFilter = Builders<SquadJoinRequest>.Filter.Gt("id", lastId);
        var userFilter = Builders<SquadJoinRequest>.Filter.Eq("SenderUserId", userId);
        
        return await _joinRequests
            .Find(userFilter & paginationFilter)
            .Limit(20)
            .ToListAsync();
    }
    
    /// <summary>
    /// Returns the join requests a squad has received
    /// Returns a up to 20 item enumerable. To get the next page pass the last id.
    /// </summary>
    /// <param name="squadId">The squad id you want to get join requests from</param>
    /// <param name="lastId">Last request of previous page served. This can be null.</param>
    /// <returns></returns>
    public async Task<IEnumerable<SquadJoinRequest>> GetJoinRequestsOfSquad(Guid squadId, string lastId = null)
    {
        if (lastId == null)
            return await _joinRequests
                .Find(joinReq => joinReq.SquadId.Equals(squadId))
                .Limit(20)
                .ToListAsync();
        
        var paginationFilter = Builders<SquadJoinRequest>.Filter.Gt("id", lastId);
        var squadFilter = Builders<SquadJoinRequest>.Filter.Eq("SquadId", squadId);
        
        return await _joinRequests
            .Find(squadFilter & paginationFilter)
            .Limit(20)
            .ToListAsync();
    }

    /// <summary>
    /// Returns the join requests a given user has received in all of the squads this admins
    /// Returns a up to 20 item enumerable. To get the next page pass the last id.
    /// </summary>
    /// <returns>IEnumerable<see cref="SquadJoinRequest"/></returns>
    public async Task<IEnumerable<SquadJoinRequest>> GetJoinRequestsForUser(int userId, string lastId = null)
    {
        // As join requests don't store the userId of the user that receives, I need to retrieve the ids of the
        // squads that the user admins.

        var squads = _squads.GetSquadsUserOwns(userId).Select(squad => squad.Id).ToArray();
        
        // Now that I have the ids, I can check the squads array against the join requests
        if (lastId == null)
            return await _joinRequests
                .Find(joinReq => squads.Contains(joinReq.SquadId))
                .Limit(20)
                .ToListAsync();
        
        var paginationFilter = Builders<SquadJoinRequest>.Filter.Gt("id", lastId);
        var userFilter = Builders<SquadJoinRequest>.Filter.Eq("SenderUserId", userId);
        
        return await _joinRequests
            .Find(userFilter & paginationFilter)
            .Limit(20)
            .ToListAsync();
    }

    public async Task<SquadJoinRequest> SearchJoinRequest(Guid squadId, int senderUserId)
    {
        return await _joinRequests.Find(joinReq =>
            joinReq.SenderUserId.Equals(senderUserId) && joinReq.SquadId.Equals(squadId)).FirstOrDefaultAsync();
    }

    public async Task<long> GetUnseenRequestsForUser(Guid[] squads)
    {
        return await _joinRequests.Find(req => squads.Contains(req.SquadId)).CountDocumentsAsync();
    }

    public async Task RemoveJoinRequestFromAndToUser(int userId)
    {
        var filterFrom = Builders<SquadJoinRequest>.Filter.Eq(si => si.SenderUserId, userId);

        await _joinRequests.DeleteManyAsync(filterFrom);
    }
}
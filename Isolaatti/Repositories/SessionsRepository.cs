using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.DTOs;
using Isolaatti.Models.MongoDB;
using Isolaatti.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Isolaatti.Repositories;

public class SessionsRepository
{
    private readonly IMongoCollection<Session> _authTokens;    
    public SessionsRepository(MongoDatabase mongoDatabase)
    {
        _authTokens = mongoDatabase.GetSessionsCollection();
    }

    public async Task<Session> InsertSession(Session session)
    {
        await _authTokens.InsertOneAsync(session);
        return session;
    }

    public async Task<IEnumerable<Session>> FindSessionsOfUser(int userId)
    {
        return (await _authTokens.FindAsync(t => t.UserId == userId)).ToEnumerable();
    }

    public async Task<int?> FindUserIdFromSession(SessionDto sessionDto) 
    {
        var session = await _authTokens.Find<Session>(session => session.Id == sessionDto.SessionId).FirstOrDefaultAsync();

        if(session == null) 
        {
            return null;
        }

        if(!session.SessionKey.Equals(sessionDto.SessionKey))
        {
            return null;
        }

        return session.UserId;
    }

    public async Task<bool> RemoveSession(SessionDto sessionDto)
    {
        var sessionRemoved = await _authTokens
            .FindOneAndDeleteAsync(session => session.Id.Equals(sessionDto.SessionId));

        return sessionRemoved != null;
    }

    public async Task<bool> RemoveSessionsByUserId(int userId, IEnumerable<string> exceptIds)
    {
        var filter = Builders<Session>.Filter.Eq(s => s.UserId, userId) &
                     Builders<Session>.Filter.Not(Builders<Session>.Filter.In(s => s.Id, exceptIds));
        var result = await _authTokens.DeleteManyAsync(filter);
        return result.IsAcknowledged;
    }

    public async Task<Session> FindSessionById(SessionDto sessionDto)
    {
        return await _authTokens
            .Find(session => session.Id.Equals(sessionDto.SessionId))
            .FirstOrDefaultAsync();
    }

    public async Task<bool> RemoveSessions(int userId, IEnumerable<string> ids)
    {
        var filter = Builders<Session>.Filter.Eq(s => s.UserId, userId) & Builders<Session>.Filter.In(s => s.Id, ids);
        var result = await _authTokens.DeleteManyAsync(filter);

        return result.IsAcknowledged;
    }
}
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
    private readonly MongoDatabaseConfiguration _settings;
    private readonly ScopedHttpContext _scopedHttpContext;
    
    public SessionsRepository(IOptions<MongoDatabaseConfiguration> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        var database = client.GetDatabase(_settings.DatabaseName);
        _authTokens = database.GetCollection<Session>(_settings.AuthTokensCollectionName);
    }

    public async Task<Session> InsertSession(Session session)
    {
        await _authTokens.InsertOneAsync(session);
        return session;
    }

    public IEnumerable<Session> FindSessionsOfUser(int userId)
    {
        return _authTokens.Find(t => t.UserId == userId).ToEnumerable();
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

    public async Task<Session> FindSessionById(SessionDto sessionDto)
    {
        return await _authTokens
            .Find(session => session.Id.Equals(sessionDto.SessionId))
            .FirstOrDefaultAsync();
    }
}
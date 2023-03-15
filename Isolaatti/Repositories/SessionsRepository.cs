using System.Collections.Generic;
using System.Threading.Tasks;
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

    public async Task<Session> InsertSession(Session token)
    {
        await _authTokens.InsertOneAsync(token);
        return token;
    }

    public IEnumerable<Session> FindSessionsOfUser(int userId)
    {
        return _authTokens.Find(t => t.UserId == userId).ToEnumerable();
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Classes.Authentication;
using Isolaatti.Models.MongoDB;
using Isolaatti.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Isolaatti.Repositories;

public class AuthTokensRepository
{
    private readonly IMongoCollection<AuthToken> _authTokens;
    private readonly MongoDatabaseConfiguration _settings;
    private readonly KeyGenService _keyGen;
    private readonly ScopedHttpContext _scopedHttpContext;
    
    public AuthTokensRepository(IOptions<MongoDatabaseConfiguration> settings, KeyGenService keyGen)
    {
        _keyGen = keyGen;
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        var database = client.GetDatabase(_settings.DatabaseName);
        _authTokens = database.GetCollection<AuthToken>(_settings.AuthTokensCollectionName);
    }

    public async Task<AuthToken> StoreToken(AuthToken token)
    {
        await _authTokens.InsertOneAsync(token);
        return token;
    }

    public async Task<AuthToken> FindToken(string id)
    {
        return await _authTokens.Find(t => t.Id.Equals(id)).FirstOrDefaultAsync();
    }

    public IEnumerable<AuthToken> FindTokenOfUser(int userId)
    {
        return _authTokens.Find(t => t.UserId == userId).ToEnumerable();
    }

    public async Task RemoveToken(string id)
    {
        await _authTokens.DeleteOneAsync(t => t.Id.Equals(id));
    }

    public async Task RemoveTokenOfUser(int userId)
    {
        await _authTokens.DeleteManyAsync(t => t.UserId.Equals(userId));
    }
}
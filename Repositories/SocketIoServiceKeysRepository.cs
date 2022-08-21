using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Isolaatti.Utils;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Isolaatti.Repositories;

public class SocketIoServiceKeysRepository
{
    private readonly IMongoCollection<SocketIoServiceKey> _socketIoKeys;
    private readonly MongoDatabaseConfiguration _settings;

    public SocketIoServiceKeysRepository(IOptions<MongoDatabaseConfiguration> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        var database = client.GetDatabase(_settings.DatabaseName);
        _socketIoKeys = database.GetCollection<SocketIoServiceKey>(_settings.RealtimeServiceKeysCollectionName);
    }

    public async Task<SocketIoServiceKey> CreateKey()
    {
        var key = new SocketIoServiceKey
        {
            Key = RandomData.GenerateRandomKey(32)
        };
        await _socketIoKeys.InsertOneAsync(key);
        return key;
    }

    public async Task RemoveKey(SocketIoServiceKey key)
    {
        await _socketIoKeys.DeleteOneAsync(doc => doc.Key == key.Key);
    }

    public async Task<bool> Exists(SocketIoServiceKey key)
    {
        var docs = _socketIoKeys.Find(k => k.Key.Equals(key.Key));
        return await docs.CountDocumentsAsync() > 0;
    }
}
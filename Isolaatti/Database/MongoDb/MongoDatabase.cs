using Isolaatti.Comments.Entity;
using Isolaatti.Images.Data;
using Isolaatti.RealtimeInteraction.Entity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Isolaatti.Database.MongoDb
{
    public class MongoDatabase
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _database;

        private readonly MongoDatabaseConfiguration _settings;
        public MongoDatabase(IOptions<MongoDatabaseConfiguration> settings)
        {
            _settings = settings.Value;
            _mongoClient = new MongoClient(_settings.ConnectionString);
            _database = _mongoClient.GetDatabase(_settings.DatabaseName);
        }
        
        public IMongoCollection<SocketIoServiceKey> GetSocketIoServiceKeysCollection()
        {
            return _database.GetCollection<SocketIoServiceKey>(_settings.RealtimeServiceKeysCollectionName);
        }

        public IMongoCollection<CommentModificationHistory> GetCommentModificationHistoryCollection()
        {
            return _database.GetCollection<CommentModificationHistory>(_settings.CommentModificationHistoryCollectionName);
        }
    }
}

using Isolaatti.Comments.Entity;
using Isolaatti.Models.MongoDB;
using Isolaatti.RealtimeInteraction.Entity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace Isolaatti.Repositories
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

        public IMongoCollection<Audio> GetAudiosCollection()
        {
            return _database.GetCollection<Audio>(_settings.AudiosCollectionName);
        }

        public IMongoCollection<Image> GetImagesCollection()
        {
            return _database.GetCollection<Image>(_settings.ImagesCollectionName);
        }

        public IMongoCollection<Session> GetSessionsCollection()
        {
            return _database.GetCollection<Session>(_settings.AuthTokensCollectionName);
        }

        public IMongoCollection<SocketIoServiceKey> GetSocketIoServiceKeysCollection()
        {
            return _database.GetCollection<SocketIoServiceKey>(_settings.RealtimeServiceKeysCollectionName);
        }

        public IMongoCollection<SquadInvitation> GetSquadInvitationsCollection()
        {
            return _database.GetCollection<SquadInvitation>(_settings.SquadsInvitationsCollectionName);
        }

        public IMongoCollection<SquadJoinRequest> GetSquadJoinRequestsCollection()
        {
            return _database.GetCollection<SquadJoinRequest>(_settings.SquadsJoinRequestsCollectionName);
        }

        public IMongoCollection<CommentModificationHistory> GetCommentModificationHistoryCollection()
        {
            return _database.GetCollection<CommentModificationHistory>(_settings.CommentModificationHistoryCollectionName);
        }
    }
}

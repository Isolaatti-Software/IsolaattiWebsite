using Isolaatti.Models.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Isolaatti.Repositories;

public class NotificationsRepository
{
    private readonly IMongoCollection<SocialNotification> _notifications;
    private readonly MongoDatabaseConfiguration _settings;

    public NotificationsRepository(IOptions<MongoDatabaseConfiguration> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        var database = client.GetDatabase(_settings.DatabaseName);
        _notifications = database.GetCollection<SocialNotification>(_settings.NotificationsCollectionName);
    }
}
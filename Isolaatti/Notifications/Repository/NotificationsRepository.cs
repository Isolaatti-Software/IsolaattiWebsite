using System;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.Models.MongoDB;
using Isolaatti.Notifications.Entity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Isolaatti.Notifications.Repository;

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
    
    public async Task InsertNotification(SocialNotification notification)
    {
        await _notifications.InsertOneAsync(notification);
    }

    public async Task MarkAsReadNotification(string notificationId)
    {
        var filter = Builders<SocialNotification>.Filter.Eq(n => n.Id, notificationId);
        var update = Builders<SocialNotification>.Update.Set(n => n.Read, true);
        await _notifications.UpdateOneAsync(filter, update);
    }

    public async Task DeleteNotificationsById(params string[] notificationId)
    {
        var filter = Builders<SocialNotification>.Filter.In(n => n.Id, notificationId);
        await _notifications.DeleteManyAsync(filter);
    }
    
    public async Task<Paginable<SocialNotification>> GetNotificationsForUser(int userId, int page)
    {
        var filter = Builders<SocialNotification>.Filter.Eq(n => n.UserId, userId);
        

        var sorting = Builders<SocialNotification>.Sort.Descending(n => n.Id);
        var result = _notifications.Find(filter);
        var count = await result.CountDocumentsAsync();

        await result.Sort(sorting)
            .Skip(page * 20)
            .Limit(20).ToListAsync();

        return new Paginable<SocialNotification>() 
        {
               CurrentPage = page,
               Pages = Convert.ToInt32(Math.Floor(count / 20.0))
        };
    }
}
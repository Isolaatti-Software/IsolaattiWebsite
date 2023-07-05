using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.Models.MongoDB;
using Isolaatti.Notifications.Entity;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Isolaatti.Notifications.Repository;

public class NotificationsRepository
{
    private readonly IMongoCollection<Notification> _notifications;
    private readonly MongoDatabaseConfiguration _settings;

    public NotificationsRepository(IOptions<MongoDatabaseConfiguration> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        var database = client.GetDatabase(_settings.DatabaseName);
        _notifications = database.GetCollection<Notification>(_settings.NotificationsCollectionName);
    }
    
    public async Task InsertNotification(Notification notification)
    {
        await _notifications.InsertOneAsync(notification);
    }

    public async Task<bool> TryInsertLikeNotificationRationally(Notification notification)
    {
        throw new NotImplementedException();
    }


    public async Task MarkAsReadNotification(string notificationId, int userId)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId) & Builders<Notification>.Filter.Eq(n => n.UserId, userId);
        var update = Builders<Notification>.Update.Set(n => n.Read, true);
        await _notifications.UpdateOneAsync(filter, update);
    }

    public async Task DeleteNotificationsById(int userId, params string[] notificationId)
    {
        var filter = Builders<Notification>.Filter.In(n => n.Id, notificationId);
        var ownerFilter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
        await _notifications.DeleteManyAsync(ownerFilter & filter);
    }
    
    public async Task<List<Notification>> GetNotificationsForUser(int userId, string after)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
        
        var sorting = Builders<Notification>.Sort.Descending(n => n.Id);

        if(after != null)
        {
            var pagination = Builders<Notification>.Filter.Gt(n => n.Id, after);
            filter &= pagination;
        }
        var result = _notifications.Find(filter);



        return await result.Sort(sorting).Limit(20).ToListAsync();
    }

    public async Task DeleteNotificationsOfUser(int userId)
    {

        await _notifications.DeleteManyAsync(n => n.UserId == userId);
    }
}
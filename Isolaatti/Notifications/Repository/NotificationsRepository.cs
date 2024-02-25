using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Isolaatti.Notifications.Entity;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Isolaatti.Notifications.Repository;

public class NotificationsRepository
{
    private readonly DbContextApp _db;
    private readonly MongoDatabaseConfiguration _settings;

    public NotificationsRepository(IOptions<MongoDatabaseConfiguration> settings, DbContextApp db)
    {
        _db = db;
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        var database = client.GetDatabase(_settings.DatabaseName);
        
    }
    
    public async Task InsertNotification(NotificationEntity notificationEntity)
    {
        _db.Notifications.Add(notificationEntity);
        await _db.SaveChangesAsync();
    }

    public bool TryInsertLikeNotificationRationally(NotificationEntity notificationEntity)
    {
        throw new NotImplementedException();
    }


    public async Task MarkAsReadNotification(long notificationId, int userId)
    {
        var notification = await _db.Notifications.FindAsync(notificationId);

        if (notification == null)
        {
            return;
        }
        
        if (notification.UserId == userId)
        {
            notification.Read = true;
            _db.Notifications.Update(notification);
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteNotificationsById(int userId, params long[] notificationId)
    {

        var notifications = _db.Notifications.Where(notification => notificationId.Contains(notification.Id) && notification.UserId == userId);
        
        _db.Notifications.RemoveRange(notifications);
        await _db.SaveChangesAsync();
    }
    
    public List<NotificationEntity> GetNotificationsForUser(int userId, long? after)
    {
        List<NotificationEntity> notifications;
        if (after == null)
        {
            notifications = _db.Notifications
                .Where(notification => notification.UserId == userId)
                .OrderByDescending(notification => notification.Id)
                .Take(20)
                .ToList();
        }
        else
        {
            notifications =
                _db.Notifications
                    .Where(notification => notification.UserId == userId && notification.Id < after)
                    .OrderByDescending(notification => notification.Id)
                    .Take(20)
                    .ToList();
        }
       

        return notifications;
    }

    public async Task DeleteNotificationsOfUser(int userId)
    {
        var notifications =
            _db.Notifications
                .Where(notification => notification.UserId == userId);
        
        _db.Notifications.RemoveRange(notifications);

        await _db.SaveChangesAsync();
    }
}
using System;
using Isolaatti.Models;
using Isolaatti.Notifications.Dto;
using Isolaatti.Notifications.Entity;
using Isolaatti.RealtimeInteraction.Service;
using Isolaatti.Users.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Urls;
using Isolaatti.Utils;

namespace Isolaatti.Notifications.Services;

public class NotificationsService
{
    private readonly NotificationSender _notificationSender;
    private readonly UsersRepository _usersRepository;
    private readonly DbContextApp _db;
    private readonly UrlsService _urlsService;
    
    public NotificationsService(NotificationSender notificationSender, UsersRepository usersRepository, UrlsService urlsService, DbContextApp db)
    {
        _notificationSender = notificationSender;
        _usersRepository = usersRepository;
        _urlsService = urlsService;
        _db = db;
    }

    public async Task DeleteNotification(int userId, params long[] ids)
    {
        var notifications = _db.Notifications.Where(notification => ids.Contains(notification.Id) && notification.UserId == userId);
        
        _db.Notifications.RemoveRange(notifications);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAllNotifications(int userId)
    {
        var notifications =
            _db.Notifications
                .Where(notification => notification.UserId == userId);
        
        _db.Notifications.RemoveRange(notifications);

        await _db.SaveChangesAsync();
    }

    public async Task MarkAsRead(long notificationId, int userId)
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

    public IEnumerable<NotificationDto> GetUserNotifications(int userId, long? after)
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

        return notifications.Select(notification =>
        {
            var dto = NotificationDto.FromEntity(notification);
            if (dto.Data == null) return dto;
            
            var authorId = dto.Data?[NotificationEntity.KeyAuthorId]?.GetValue<string>().ToInt();
            if (authorId == null) return dto;
            
            
            // If there is a "authorId" value, author name must be added always
            var authorName = _usersRepository.GetUsernameById(authorId.Value);

            dto.Data![NotificationEntity.KeyAuthorName] = JsonSerializer.SerializeToNode(authorName);


            return dto;

        });
    }

    public async Task InsertNewLikeNotification(Like like)
    {
        var notificationToInsert = new NotificationEntity()
        {
            UserId = like.TargetUserId,
            Data = JsonSerializer.SerializeToDocument(new Dictionary<string, string>()
            {
                { NotificationEntity.KeyType , NotificationEntity.TypeLike },
                { NotificationEntity.KeyAuthorId, like.UserId.ToString()},
                { NotificationEntity.KeyLikeId, like.LikeId.ToString() },
                { NotificationEntity.KeyPostId, like.PostId.ToString() }
            }),
            RelatedNotifications = Array.Empty<long>()
        };
        

        var existingNotifications = _db.Notifications
            .Where(notification =>
                notification.UserId == like.TargetUserId
                && notification.Data.RootElement.GetProperty(NotificationEntity.KeyType).GetString() ==
                NotificationEntity.TypeLike
                && notification.Data.RootElement.GetProperty(NotificationEntity.KeyPostId).GetString() ==
                like.PostId.ToString()).Select(existingNotification => existingNotification.Id).ToArray();

        if (existingNotifications.Length != 0)
        {
            notificationToInsert.RelatedNotifications = existingNotifications;
            
            _db.Notifications.RemoveRange(_db.Notifications.Where(notification => existingNotifications.Contains(notification.Id)));
            await _db.SaveChangesAsync();
        }

        
        _db.Notifications.Add(notificationToInsert);
        await _db.SaveChangesAsync();
        
        _notificationSender.NotifyUser(like.TargetUserId, notificationToInsert);
    }

    public async Task InsertNewFollowerNotification(FollowerRelation followerRelation)
    {
        var notification = new NotificationEntity()
        {
            UserId = followerRelation.TargetUserId,
            Data = JsonSerializer.SerializeToDocument(new Dictionary<string, string>()
            {
                { NotificationEntity.KeyFollowerUserId, followerRelation.UserId.ToString() }
            })
        };

        
    }

    public async Task InsertNewUserActivityNotification(int userId, long postId)
    {
        var notification = new NotificationEntity()
        {
            UserId = userId,
            Data = JsonSerializer.SerializeToDocument(new Dictionary<string, string>()
            {
                { NotificationEntity.KeyPostId, postId.ToString()}
            })
        };

        
    }
}
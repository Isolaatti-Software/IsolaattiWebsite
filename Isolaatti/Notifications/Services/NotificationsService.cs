using Isolaatti.Models;
using Isolaatti.Notifications.Dto;
using Isolaatti.Notifications.Entity;
using Isolaatti.Notifications.Repository;
using Isolaatti.RealtimeInteraction.Service;
using Isolaatti.Users.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Urls;
using static Isolaatti.Notifications.Dto.NotificationDto;
using static Isolaatti.Notifications.Entity.NotificationEntity;

namespace Isolaatti.Notifications.Services;

public class NotificationsService
{
    private readonly NotificationsRepository _notificationsRepository;
    private readonly NotificationSender _notificationSender;
    private readonly UsersRepository _usersRepository;
    private readonly UrlsService _urlsService;
    
    public NotificationsService(NotificationsRepository notificationsRepository, NotificationSender notificationSender, UsersRepository usersRepository, UrlsService urlsService)
    {
        _notificationsRepository = notificationsRepository;
        _notificationSender = notificationSender;
        _usersRepository = usersRepository;
        _urlsService = urlsService;
    }

    public async Task DeleteNotification(int userId, params long[] ids)
    {
        await _notificationsRepository.DeleteNotificationsById(userId, ids);
    }

    public async Task DeleteAllNotifications(int userId)
    {
        await _notificationsRepository.DeleteNotificationsOfUser(userId);
    }

    public async Task MarkAsRead(long notificationId, int userId)
    {
        await _notificationsRepository.MarkAsReadNotification(notificationId, userId);
    }

    public IEnumerable<NotificationDto> GetUserNotifications(int userId, long? after)
    {
        var notifications = _notificationsRepository.GetNotificationsForUser(userId, after);

        return notifications.Select(notification =>
        {
            var dto = NotificationDto.FromEntity(notification);
            dto.Payload.AuthorName = _usersRepository.GetUsernameById(dto.Payload.AuthorId) ?? "";
            return dto;
        });
    }

    public async Task InsertNewLikeNotification(Like like)
    {
        var notification = new NotificationEntity()
        {
            UserId = like.TargetUserId,
            Payload = new NotificationPayloadEntity()
            {
                Type = NotificationPayloadEntity.TypeLike,
                AuthorId = like.UserId,
                IntentData = like.PostId.ToString()
            }
        };

        await _notificationsRepository.InsertNotification(notification);
    }

    public async Task InsertNewFollowerNotification(FollowerRelation followerRelation)
    {
        var notification = new NotificationEntity()
        {
            UserId = followerRelation.TargetUserId,
            Payload = new NotificationPayloadEntity()
            {
                
            }
        };

        await _notificationsRepository.InsertNotification(notification);
    }

    public void InsertNewUserActivityNotification(int userId, long postId)
    {
        var notification = new NotificationEntity()
        {
            UserId = userId,
            Payload = new NotificationPayloadEntity()
            {
                
            }
        };

        _notificationsRepository.InsertNotification(notification);
    }
}
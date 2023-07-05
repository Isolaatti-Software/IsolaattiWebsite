using Isolaatti.Models;
using Isolaatti.Notifications.Dto;
using Isolaatti.Notifications.Entity;
using Isolaatti.Notifications.Repository;
using Isolaatti.RealtimeInteraction.Service;
using Isolaatti.Users;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Isolaatti.Notifications.Dto.NotificationDto;
using static Isolaatti.Notifications.Entity.Notification;

namespace Isolaatti.Notifications.Services;

public class NotificationsService
{
    private readonly NotificationsRepository _notificationsRepository;
    private readonly NotificationSender _notificationSender;
    private readonly UsersRepository _usersRepository;
    
    public NotificationsService(NotificationsRepository notificationsRepository, NotificationSender notificationSender, UsersRepository usersRepository)
    {
        _notificationsRepository = notificationsRepository;
        _notificationSender = notificationSender;
        _usersRepository = usersRepository;
    }

    public async Task DeleteNotification(int userId, params string[] ids)
    {
        await _notificationsRepository.DeleteNotificationsById(userId, ids);
    }

    public async Task DeleteAllNotifications(int userId)
    {
        await _notificationsRepository.DeleteNotificationsOfUser(userId);
    }

    public async Task MarkAsRead(string notificationId, int userId)
    {
        await _notificationsRepository.MarkAsReadNotification(notificationId, userId);
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotifications(int userId, string after)
    {
        var notifications = await _notificationsRepository.GetNotificationsForUser(userId, after);

        return notifications.Select(n =>
        {
            var notificationDto = new NotificationDto
            {
                Id = n.Id,
                Date = n.TimeStamp,
                UserId = n.UserId,
                ImageId = _usersRepository.GetUserImageId(n.UserId),
                Read = n.Read,
                Payload = n.Payload switch
                {
                    LikeNotificationPayload likeNotificationPayload =>
                        new LikePayloadDto
                        {
                            LikeId = likeNotificationPayload.LikeId,
                            MakerUserId = likeNotificationPayload.MakerUserId,
                            Type = likeNotificationPayload.Type,
                            MakerUserName = _usersRepository.GetUsername(likeNotificationPayload.MakerUserId)
                        },
                    FollowerNotificationPayload followerNotificationPayload =>
                        new FollowerPayloadDto
                        {
                            NewFollowerUserId = followerNotificationPayload.NewFollowerUserId,
                            NewFollowerName = _usersRepository.GetUsername(followerNotificationPayload.NewFollowerUserId)
                        },

                    InformativeMessageNotificationPayload informativeMessageNotificationPayload =>
                        new InformativeMessageNotificationPayload
                        {
                            Url = informativeMessageNotificationPayload.Url,
                            ImageUrl = informativeMessageNotificationPayload.ImageUrl,
                            Text = informativeMessageNotificationPayload.Text,
                            Type = informativeMessageNotificationPayload.Type
                        },


                    NewActivityOnPost newActivityOnPostPayload => new NewActivityOnPostDto { },
                    NewActivityOfUser newActivityOfUserPayload => new NewActivityOfUserDto { },
                    NewSquadJoinRequest newSquadJoinRequestPayload => new NewSquadJoinRequestDto { },
                    NewSquadInvitation newSquadInvitationPayload => new InformativeMessageNotificationPayload { },
                    NewActivityOnSquad newActivityOnSquadPayload => new InformativeMessageNotificationPayload { },
                    _ => n.Payload
                }
            };

            return notificationDto;
        });
    }

    public async Task InsertNewLikeNotification(Like like)
    {
        var notification = new Notification()
        {
            UserId = like.TargetUserId,
            Payload = new LikeNotificationPayload()
            {
                LikeId = like.LikeId,
                MakerUserId = like.UserId
            }
        };

        await _notificationsRepository.InsertNotification(notification);
    }

    public async Task InsertNewFollowerNotification(FollowerRelation followerRelation)
    {
        var notification = new Notification()
        {
            UserId = followerRelation.TargetUserId,
            Payload = new FollowerNotificationPayload()
            {
                NewFollowerUserId = followerRelation.UserId
            }
        };

        await _notificationsRepository.InsertNotification(notification);
    }

    public async Task InsertNewUserActivityNotification(int userId, long postId)
    {
        var notification = new Notification()
        {
            UserId = userId,
            Payload = new NewActivityOfUser()
            {
                PostId = postId
            }
        };

        await _notificationsRepository.InsertNotification(notification);
    }
}
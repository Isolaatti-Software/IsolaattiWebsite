using Isolaatti.Notifications.Repository;
using Isolaatti.RealtimeInteraction.Service;

namespace Isolaatti.Notifications.Services;

public class NotificationsService
{
    private readonly NotificationsRepository _notificationsRepository;
    private readonly NotificationSender _notificationSender;
    
    public NotificationsService(NotificationsRepository notificationsRepository, NotificationSender notificationSender)
    {
        _notificationsRepository = notificationsRepository;
        _notificationSender = notificationSender;
    }
}
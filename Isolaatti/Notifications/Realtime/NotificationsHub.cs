using System.Threading.Tasks;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.SignalR;

namespace Isolaatti.Notifications.Realtime;

public class NotificationsHub : Hub
{
    [IsolaattiAuth]
    public async Task AddNotificationToTop(int userId)
    {
        
    }
}
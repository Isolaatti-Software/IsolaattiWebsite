using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace isolaatti_API.Classes
{
    public class NotificationsHub : Hub
    {
        public Task SendMessage(int userId, int notificationType)
        {
            return Clients.User(userId.ToString()).SendAsync("NewNotification", notificationType);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.SignalR;

namespace isolaatti_API.Hubs
{
    public class NotificationsHub : Hub
    {
        public static Dictionary<string, Guid> Sessions = new Dictionary<string, Guid>();
        private readonly DbContextApp _db;
        
        public NotificationsHub(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        public override Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var sessionId = httpContext.Request.Cookies["isolaatti_user_session_token"];
            var accounts = new Accounts(_db);
            var user = accounts.ValidateToken(sessionId);

            Sessions.Add(Context.ConnectionId, user.Id);
            
            return Clients.Caller.SendAsync("SessionSaved",
                $"Welcome to notifications hub {user.Name}, your temp id is {Context.ConnectionId}");
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Sessions.Remove(Sessions.Single(element => element.Key.Equals(Context.ConnectionId)).Key);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
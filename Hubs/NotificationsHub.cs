using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.SignalR;

namespace isolaatti_API.Hubs
{
    public class NotificationsHub : Hub
    {
        public static Dictionary<int, List<string>> Sessions = new Dictionary<int, List<string>>();
        private readonly DbContextApp _db;
        
        public NotificationsHub(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        public Task EstablishConnection()
        {
            var httpContext = Context.GetHttpContext();
            var sessionId = httpContext.Request.Cookies["isolaatti_user_session_token"];
            var accounts = new Accounts(_db);
            var user = accounts.ValidateToken(sessionId);
            if (Sessions.ContainsKey(user.Id))
            {
                Sessions[user.Id].Add(Context.ConnectionId);
            }
            else
            {
                Sessions[user.Id] = new List<string> {Context.ConnectionId};
            }

            return Clients.Caller.SendAsync("SessionSaved",
                $"Welcome to notifications hub {user.Name}, your temp id is {Context.ConnectionId}");
        }
        
    }
}
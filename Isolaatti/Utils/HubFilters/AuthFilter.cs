using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Database.EF;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Isolaatti.Utils.HubFilters;

public class AuthFilter(ILogger<AuthFilter> logger, IAccountsService accounts, DbContextApp db) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        
        
        
        
        return await next(invocationContext);
    }

    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        logger.LogDebug("OnConnected {}", context.Context.ConnectionId);
        var accessToken = context.Context.GetHttpContext()?.Request.Query["access_token"].First();
        if (accessToken == null)
        {
            throw new AuthenticationException();
        }
        return next(context);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        logger.LogDebug("OnDisconnect  {}", context.Context.ConnectionId);
        // eliminar registro de conexion si existe
        return next(context, exception);
    }
}
﻿using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.DTOs;
using Isolaatti.Services;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Isolaatti.Utils.ActionFilters;

public class AuthenticationFilter : IAsyncActionFilter
{
    private readonly IAccountsService _accounts;
    public AuthenticationFilter(IAccountsService accounts)
    {
        _accounts = accounts;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var attrs = context.ActionDescriptor.EndpointMetadata.OfType<IsolaattiAuth>();
        if (attrs.Any())
        {
            // Api requests can be authenticated by Authorization header or cookie value isolaatti_user_session_token
            
            var header = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            var cookie = context.HttpContext.Request.Cookies[AccountsService.SessionCookieName];
            
            if (header == null && cookie == null)
            {
                context.Result = new UnauthorizedObjectResult("Unauthorized");
                return;
            }

            SessionDto session;

            try
            {
                session = JsonSerializer.Deserialize<SessionDto>(header ?? cookie);
            }
            catch (JsonException)
            {
                context.Result = new BadRequestResult();
                return;
            }

            var user = await _accounts.ValidateSession(session);
            if (user == null)
            {
                context.Result = new UnauthorizedObjectResult("Unauthorized");
                return;
            }

            if (context.Controller is IsolaattiController controller)
            {
                controller.User = user;
                controller.CurrentSessionDto = session;
            }
            await next();
        }
        else
        {
            await next();
        }
    }
}
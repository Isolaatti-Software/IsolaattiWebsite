using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Accounts.SignIn.Dto;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Isolaatti.Utils.ActionFilters;

public class AuthenticationFilter : IAsyncAuthorizationFilter
{
    private readonly IAccountsService _accounts;

    public AuthenticationFilter(IAccountsService accounts)
    {
        _accounts = accounts;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var attrs = context.ActionDescriptor.EndpointMetadata
            .OfType<IsolaattiAuth>();

        if (!attrs.Any())
        {
            return;
        }

        // Authorization header OR session cookie
        var header = context.HttpContext.Request.Headers.Authorization
            .FirstOrDefault();

        var cookie = context.HttpContext.Request.Cookies[
            AccountsService.SessionCookieName];

        var token = header ?? cookie;

        if (token == null)
        {
            context.Result = new UnauthorizedObjectResult("Unauthorized");
            return;
        }

        var session = await _accounts.GetSessionById(token);
        if (session == null)
        {
            context.Result = new UnauthorizedObjectResult("Unauthorized");
            return;
        }

        var user = await _accounts.ValidateSession(session);
        if (user == null)
        {
            context.Result = new UnauthorizedObjectResult("Unauthorized");
            return;
        }
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("session_id", session.Id)
        };

        var identity = new ClaimsIdentity(claims, "Isolaatti");
        var principal = new ClaimsPrincipal(identity);

        context.HttpContext.User = principal;
        context.HttpContext.Items["CurrentUser"] = user;
        context.HttpContext.Items["CurrentSession"] = session;
    }
}

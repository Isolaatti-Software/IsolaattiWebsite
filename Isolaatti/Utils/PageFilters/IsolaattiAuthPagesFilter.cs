using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.DTOs;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Isolaatti.Utils.PageFilters;

public class IsolaattiAuthPagesFilter : IAsyncPageFilter
{
    private IAccountsService _accounts;
    private ILogger<IsolaattiAuthPagesFilter> _logger;
    
    public IsolaattiAuthPagesFilter(IAccountsService accounts, ILogger<IsolaattiAuthPagesFilter> logger)
    {
        _accounts = accounts;
        _logger = logger;
    }
    
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        
        return Task.CompletedTask;
    }

    private void RedirectToLogin(PageHandlerExecutingContext context)
    {
        _logger.LogDebug("Then {then}", context.HttpContext.Request.Path);
        context.Result = new RedirectToPageResult("/LogIn",
            new { then = context.HttpContext.Request.Path });
    }
    
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var attrs = context.ActionDescriptor.EndpointMetadata.OfType<IsolaattiAuth>();
        if (attrs.Any())
        {
            var cookie = context.HttpContext.Request.Cookies["isolaatti_user_session_token"];
            if (cookie == null)
            {
                _logger.LogDebug("Cookie not present, redirecting to log");
                RedirectToLogin(context);
                return;
            }

            try
            {
                var session = JsonSerializer.Deserialize<SessionDto>(cookie);
                var user = await _accounts.ValidateSession(session);
                if (user == null)
                {
                    _logger.LogDebug("User is null");
                    RedirectToLogin(context);
                    return;
                }

                if (context.HandlerInstance is IsolaattiPageModel pageModel)
                {
                    pageModel.HideNav = context.HttpContext.Request.Cookies["isolaatti_hidenav"] == "yes";
                    pageModel.User = user;
                    pageModel.ViewData[IsolaattiPageModel.ViewDataNameKey] = user.Name;
                    pageModel.ViewData[IsolaattiPageModel.ViewDataEmailKey] = user.Email;
                    pageModel.ViewData[IsolaattiPageModel.ViewDataUserIdKey] = user.Id;
                    pageModel.ViewData[IsolaattiPageModel.ViewDataProfilePicUrlKey] =
                        UrlGenerators.GenerateProfilePictureUrl(user.Id, null);
                    pageModel.ViewData[IsolaattiPageModel.ViewDataJwtTokenKey] = cookie;
                }
                await next.Invoke();
            }
            catch (JsonException)
            {
                _accounts.RemoveSessionCookie();
                context.Result = new RedirectToPageResult("LogIn");
            }
            
            
        }
        else
        {
            await next.Invoke();
        }
        
    }
}
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.DTOs;
using Isolaatti.Services;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Isolaatti.Utils.PageFilters;

public class IsolaattiAuthPagesFilter : IAsyncPageFilter
{
    private readonly IAccounts _accounts;
    
    public IsolaattiAuthPagesFilter(IAccounts accounts)
    {
        _accounts = accounts;
    }
    
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        
        return Task.CompletedTask;
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
                context.Result = new RedirectToPageResult("LogIn");
                return;
            }

            try
            {
                var session = JsonSerializer.Deserialize<SessionDto>(cookie);
                var user = await _accounts.ValidateSession(session);
                if (user == null)
                {
                    context.Result = new RedirectToPageResult("LogIn",
                        new { then = context.ActionDescriptor.Endpoint });
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
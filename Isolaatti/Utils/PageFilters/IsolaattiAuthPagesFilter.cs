using System.Linq;
using System.Threading.Tasks;
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
            
            var user = await _accounts.ValidateToken(cookie);
            if (user == null)
            {
                context.Result = new RedirectToPageResult("LogIn");
                return;
            }

            if (context.HandlerInstance is IsolaattiPageModel pageModel)
            {
                pageModel.User = user;
                pageModel.ViewData[IsolaattiPageModel.ViewDataNameKey] = user.Name;
                pageModel.ViewData[IsolaattiPageModel.ViewDataEmailKey] = user.Email;
                pageModel.ViewData[IsolaattiPageModel.ViewDataUserIdKey] = user.Id;
                pageModel.ViewData[IsolaattiPageModel.ViewDataProfilePicUrlKey] = user.ProfileImageId == null
                    ? null
                    : UrlGenerators.GenerateProfilePictureUrl(user.Id, null);
                pageModel.ViewData[IsolaattiPageModel.ViewDataJwtTokenKey] = cookie;
            }

            await next.Invoke();
        }
        else
        {
            await next.Invoke();
        }
        
    }
}
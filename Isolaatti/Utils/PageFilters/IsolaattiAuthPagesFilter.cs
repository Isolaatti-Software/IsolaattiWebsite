using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Accounts.SignIn.Dto;
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
    
    
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var subClaim = context.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
        var emailClaim = context.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email);
        if (subClaim != null)
        {
            var user = await _accounts.GetUserFromSub(subClaim);


            if (user != null && context.HandlerInstance is IsolaattiPageModel pageModel)
            {
                pageModel.HideNav = context.HttpContext.Request.Cookies["isolaatti_hidenav"] == "yes";
                pageModel.User = user;
                pageModel.ViewData[IsolaattiPageModel.ViewDataNameKey] = user.Name;
                pageModel.ViewData[IsolaattiPageModel.ViewDataEmailKey] = emailClaim?.Value;
                pageModel.ViewData[IsolaattiPageModel.ViewDataUserIdKey] = user.Id;
                pageModel.ViewData[IsolaattiPageModel.ViewDataProfilePicUrlKey] =
                    UrlGenerators.GenerateProfilePictureUrl(user.Id);
            }
        }
        
        await next.Invoke();
        
    }
}
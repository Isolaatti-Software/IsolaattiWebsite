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

public class AuthenticationFilter : IAsyncActionFilter
{
    private readonly IAccountsService _accounts;

    public AuthenticationFilter(IAccountsService accounts)
    {
        _accounts = accounts;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var subClaim = context.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
        if (subClaim != null)
        {
            var user = await _accounts.GetUserFromSub(subClaim);
            context.HttpContext.Items["CurrentUser"] = user;
        }
        
        await next();
    }
}

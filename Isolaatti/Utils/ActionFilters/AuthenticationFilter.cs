using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Services;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Isolaatti.Utils.ActionFilters;

public class AuthenticationFilter : IAsyncActionFilter
{
    private readonly IAccounts _accounts;
    public AuthenticationFilter(IAccounts accounts)
    {
        _accounts = accounts;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var attrs = context.ActionDescriptor.EndpointMetadata.OfType<IsolaattiAuth>();
        if (attrs.Any())
        {
            var header = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (header == null)
            {
                context.Result = new UnauthorizedObjectResult("Unauthorized");
                await next();
            }

            var user = await _accounts.ValidateToken(header);
            if (user == null)
            {
                context.Result = new UnauthorizedObjectResult("Unauthorized");
            }

            if (context.Controller is IsolaattiController controller)
            {
                controller.User = user;
            }

            await next();
        }
        else
        {
            await next();
        }
    }
}
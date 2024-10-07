
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Isolaatti.Database.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Utils.ActionFilters;

/// <summary>
/// Read <c>User</c> claims and in case user is authenticated and controller is a <c>IsolaattiController</c>, look up
/// the profile on the database. If a profile is found, Profile field in <c>IsolaattiController</c>
/// </summary>
/// <param name="db">DbContextApp injected by DI</param>
public class AuthenticationFilter(DbContextApp db) : IAsyncActionFilter
{
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        
        if (context.Controller is ProfileRequiredController isolaattiController)
        {
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var profile = await db.Users.FirstOrDefaultAsync(profile => profile.SID.Equals(userId));
                if (profile != null)
                {
                    isolaattiController.Profile = profile;
                }
                else
                {
                    context.Result = new ConflictResult();
                    return;
                }
                
            }
        }
        await next();
    }
}
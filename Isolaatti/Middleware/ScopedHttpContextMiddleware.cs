using System.Threading.Tasks;
using Isolaatti.Common;
using Microsoft.AspNetCore.Http;

namespace Isolaatti.Middleware;

public class ScopedHttpContextMiddleware
{
    private readonly RequestDelegate _next;

    public ScopedHttpContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context, ScopedHttpContext scopedContext)
    {
        scopedContext.HttpContext = context;
        return _next(context);
    }
}
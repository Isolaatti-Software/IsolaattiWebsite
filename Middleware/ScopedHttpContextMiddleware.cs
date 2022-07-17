using System.Threading.Tasks;
using isolaatti_API.Services;
using Microsoft.AspNetCore.Http;

namespace isolaatti_API.Middleware;

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
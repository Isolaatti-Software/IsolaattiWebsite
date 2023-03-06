using Microsoft.AspNetCore.Http;

namespace Isolaatti.Services;

public class ScopedHttpContext
{
    public HttpContext HttpContext { get; set; }
}
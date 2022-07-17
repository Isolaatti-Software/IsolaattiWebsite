using Microsoft.AspNetCore.Http;

namespace isolaatti_API.Services;

public class ScopedHttpContext
{
    public HttpContext HttpContext { get; set; }
}
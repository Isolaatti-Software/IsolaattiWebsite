using Microsoft.AspNetCore.Http;

namespace Isolaatti.Common;

public class ScopedHttpContext
{
    public HttpContext HttpContext { get; set; }
}
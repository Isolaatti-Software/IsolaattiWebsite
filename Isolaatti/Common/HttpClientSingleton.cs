using System;
using System.Net.Http;

namespace Isolaatti.Common;

public class HttpClientSingleton
{
    public HttpClient Client { get; set; }

    public HttpClientSingleton()
    {
        var socketHandler = new SocketsHttpHandler()
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)
        };
        Client = new HttpClient(socketHandler);
    }
}
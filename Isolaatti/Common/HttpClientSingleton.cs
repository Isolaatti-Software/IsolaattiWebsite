using System;
using System.Net.Http;

namespace Isolaatti.Common;

public class HttpClientSingleton
{
    public HttpClient Client { get; set; }

    public HttpClientSingleton()
    {
        var httpClientHandler = new HttpClientHandler();
        httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
        {
            return true;
        };
        
        Client = new HttpClient(httpClientHandler);
    }
}
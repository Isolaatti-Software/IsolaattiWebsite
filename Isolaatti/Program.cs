using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Isolaatti
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var port = "5000";
                    if (Environment.GetEnvironmentVariable("PORT") != null)
                    {
                        port = Environment.GetEnvironmentVariable("PORT");
                    }

                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://*:" + port);
                });
    }
}
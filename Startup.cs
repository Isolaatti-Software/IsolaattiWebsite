/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.IO;
using System.Net;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using isolaatti_API.Hubs;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace isolaatti_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var file = File.Open("isolaatti-firebase-adminsdk.json", FileMode.Open);
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromStream(file)
            });
            services.AddControllers();
            services.AddRazorPages();
            services.AddSignalR();

            services.AddDbContext<DbContextApp>(options =>
            {
                //options.UseSqlite(Configuration.GetConnectionString("Database"));
                options.UseSqlServer(Configuration.GetConnectionString("Database"));
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                // No consent check needed here
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                // options.KnownProxies.Add(IPAddress.Parse("159.203.108.20"));
                options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseAuthorization();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapHub<NotificationsHub>("/notifications_hub");
            });
            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }
    }
}

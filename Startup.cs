/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System;
using System.IO;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using isolaatti_API.Hubs;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendGrid.Extensions.DependencyInjection;

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
            services.AddMvcCore().AddApiExplorer();
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddSignalR();
            bool isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != null;

            services.AddDbContext<DbContextApp>(options =>
            {
                // db-connection-string env variable must be set on production
                options.UseSqlServer(isDev
                    ? Configuration.GetConnectionString("Database")
                    : Environment.GetEnvironmentVariable("db-connection-string"));
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                // No consent check needed here
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto |
                                           ForwardedHeaders.XForwardedHost;
            });

            // send-grid-api-key env variable must be set on production
            services.AddSendGrid(options =>
            {
                options.ApiKey = isDev
                    ? Configuration.GetSection("ApiKeys")["SendGrid"]
                    : Environment.GetEnvironmentVariable("send-grid-api-key");
            });

            // don't allow uploading files larger than 2 MB, for security reasons
            services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 1024 * 1024 * 2);

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseForwardedHeaders();
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
        }
    }
}
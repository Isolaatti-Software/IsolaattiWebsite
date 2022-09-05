using System;
using System.IO;
using System.Text.Json;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Isolaatti.Config;
using Isolaatti.Middleware;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using SendGrid.Extensions.DependencyInjection;

namespace Isolaatti
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            _environment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        private IWebHostEnvironment _environment { get; }

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
            services.AddDbContext<DbContextApp>(options =>
            {
                if (_environment.IsProduction())
                {
                    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                    var databaseUri = new Uri(databaseUrl!);
                    var credentialInfo = databaseUri.UserInfo.Split(":"); // first part is user, second part is password

                    var connectionStringBuilder = new NpgsqlConnectionStringBuilder
                    {
                        Host = databaseUri.Host,
                        Port = databaseUri.Port,
                        Username = credentialInfo[0],
                        Password = credentialInfo[1],
                        Database = databaseUri.LocalPath.TrimStart('/')
                    };
                    options.UseNpgsql(connectionStringBuilder.ToString());
                }
                else
                {
                    options.UseNpgsql(Configuration.GetConnectionString("Database"));
                }
            });
            services.AddDbContext<MyKeysDbContext>(options =>
            {
                if (_environment.IsProduction())
                {
                    var databaseUrl = Environment.GetEnvironmentVariable("HEROKU_POSTGRESQL_BLACK_URL");
                    var databaseUri = new Uri(databaseUrl!);
                    var credentialInfo = databaseUri.UserInfo.Split(":"); // first part is user, second part is password

                    var connectionStringBuilder = new NpgsqlConnectionStringBuilder
                    {
                        Host = databaseUri.Host,
                        Port = databaseUri.Port,
                        Username = credentialInfo[0],
                        Password = credentialInfo[1],
                        Database = databaseUri.LocalPath.TrimStart('/')
                    };
                    options.UseNpgsql(connectionStringBuilder.ToString());
                }
                else
                {
                    options.UseNpgsql(Configuration.GetConnectionString("KeysDatabase"));
                }
            });
            services.AddDataProtection().PersistKeysToDbContext<MyKeysDbContext>();
            if (_environment.IsProduction())
            {
                var mongoConfigEnvVar = Environment.GetEnvironmentVariable("mongodb_config");
                services.Configure<MongoDatabaseConfiguration>(config =>
                {
                    var mongoConfig = JsonSerializer.Deserialize<MongoDatabaseConfiguration>(mongoConfigEnvVar!);
                    config.ConnectionString = mongoConfig?.ConnectionString;
                    config.DatabaseName = mongoConfig?.DatabaseName;
                    config.AudiosCollectionName = mongoConfig?.AudiosCollectionName;
                    config.NotificationsCollectionName = mongoConfig?.NotificationsCollectionName;
                    config.SquadsInvitationsCollectionName = mongoConfig?.SquadsInvitationsCollectionName;
                    config.SquadsJoinRequestsCollectionName = mongoConfig?.SquadsJoinRequestsCollectionName;
                    config.RealtimeServiceKeysCollectionName = mongoConfig?.RealtimeServiceKeysCollectionName;
                });
                
                var serversConfigEnvVar = Environment.GetEnvironmentVariable("servers");
                services.Configure<Servers>(config =>
                {
                    var serversConfig = JsonSerializer.Deserialize<Servers>(serversConfigEnvVar!);
                    config.RealtimeServerUrl= serversConfig?.RealtimeServerUrl;
                });
            }
            else
            {
                services.Configure<MongoDatabaseConfiguration>(Configuration.GetSection("MongoDb"));
                services.Configure<Servers>(Configuration.GetSection("Servers"));
            }
            services.AddScoped<AudiosRepository>();
            services.AddScoped<SquadInvitationsRepository>();
            services.AddScoped<SquadsRepository>();
            services.AddScoped<SquadJoinRequestsRepository>();
            services.AddScoped<SocketIoServiceKeysRepository>();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // No consent check needed here
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto |
                                           ForwardedHeaders.XForwardedHost;
            });
            // send-grid-api-key env variable must be set on production
            services.AddSendGrid(options =>
            {
                options.ApiKey = _environment.IsDevelopment()
                    ? Configuration.GetSection("ApiKeys")["SendGrid"]
                    : Environment.GetEnvironmentVariable("send_grid_api_key");
            });
            services.AddScoped<ScopedHttpContext>();
            services.AddScoped<IAccounts, Accounts>();
            services.AddScoped<NotificationSender>();
            services.AddScoped<ServerRenderedAlerts>();
            // don't allow uploading files larger than 2 MB, for security reasons
            services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 1024 * 1024 * 2);
            services.AddSwaggerGen();
            services.AddWebOptimizer(pipeline =>
            {
                pipeline.MinifyCssFiles();
                pipeline.MinifyJsFiles();
                pipeline.MinifyHtmlFiles();
                pipeline.AddScssBundle("/css/main.css", "scss/isolaatti.scss");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbContextApp dbContext,
            MyKeysDbContext keysDb, IOptions<Servers> servers)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            dbContext.Database.Migrate();
            keysDb.Database.Migrate();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseForwardedHeaders();
            app.UseHsts();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseWebOptimizer();
            app.UseHttpsRedirection();
            app.UseMiddleware<ScopedHttpContextMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.Map("/js/lib/{path}", delegate(HttpContext context)
                {
                    var path = context.Request.Path;
                    context.Response.Redirect($"{servers.Value.StaticResources}{path}", true);
                });
            });
        }
    }
}
using System;
using System.IO;
using System.Text.Json;
using EFCoreSecondLevelCacheInterceptor;
// using EFCoreSecondLevelCacheInterceptor;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Isolaatti.Auth.Config;
using Isolaatti.Config;
using Isolaatti.Middleware;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Isolaatti.Utils.ActionFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        
        private string TransformMySqlInAppConnectionString(string original)
        {
            if (original == null)
            {
                return null;
            }

            var host = "";
            var user = "";
            var password = "";
            var port = "";

            foreach (var comp in original.Split(';'))
            {
                if (comp.Contains("Data Source"))
                {
                    var hostWithPort = comp.Split("=")[1];
                    host = hostWithPort.Split(":")[0];
                    port = hostWithPort.Split(":")[1];
                }
                if (comp.Contains("Password"))
                {
                    password = comp.Split("=")[1];
                }

                if (comp.Contains("User Id"))
                {
                    user = comp.Split("=")[1];
                }
            }

            return $"server={host};port={port};database=localdb;user={user};password={password}";
        }


        public IConfiguration Configuration { get; }
        private IWebHostEnvironment _environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            const string filePath = "isolaatti-firebase-adminsdk.json";
            GoogleCredential credential;
            if (File.Exists(filePath))
            {
                credential = GoogleCredential.FromStream(File.Open(filePath, FileMode.Open));
            }
            else
            {
                var json = Environment.GetEnvironmentVariable("google_admin_sdk");
                if(json != null)
                    credential = GoogleCredential.FromJson(json);
                else
                {
                    throw new FileNotFoundException(
                        "Google credential file was not found. Tried to use env vars but did not have success.");
                }
            }
            
            FirebaseApp.Create(new AppOptions()
            {
                Credential = credential
            });
            services.AddCors(options => 
                options.AddPolicy(name: "cors",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:8100", "http://10.0.0.9:8100")
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    })
            );
            services.AddControllers(options =>
            {
                options.Filters.Add<AuthenticationFilter>();
            });
            services.AddMvcCore().AddApiExplorer();
            services.AddRazorPages().AddRazorRuntimeCompilation();
            
            services.AddWebOptimizer(options =>
            {
                options.EnableMemoryCache = true;
            });

            services.AddDbContextPool<DbContextApp>((serviceProvider, options) =>
            {
         
                if (_environment.IsProduction())
                {
                    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                    var databaseUri = new Uri(databaseUrl!);
                    var credentialInfo = databaseUri.UserInfo.Split(":"); // first part is user, second part is password
                    
                    var connectionStringBuilder = new NpgsqlConnectionStringBuilder()
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
                services.AddDataProtection().PersistKeysToDbContext<DbContextApp>();
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
                    config.AuthTokensCollectionName = mongoConfig?.AuthTokensCollectionName;
                    config.ImagesCollectionName = mongoConfig?.ImagesCollectionName;
                });
                
                var serversConfigEnvVar = Environment.GetEnvironmentVariable("servers");
                services.Configure<Servers>(config =>
                {
                    var serversConfig = JsonSerializer.Deserialize<Servers>(serversConfigEnvVar!);
                    config.RealtimeServerUrl= serversConfig?.RealtimeServerUrl;
                });
                var recaptchaConfigEnvVar = Environment.GetEnvironmentVariable("recaptcha");
                services.Configure<ReCaptchaConfig>(config =>
                {
                    var recaptchaConfig = JsonSerializer.Deserialize<ReCaptchaConfig>(recaptchaConfigEnvVar);
                    config.Site = recaptchaConfig.Site;
                    config.Secret = recaptchaConfig.Secret;
                });
            }
            else
            {
                services.Configure<MongoDatabaseConfiguration>(Configuration.GetSection("MongoDb"));
                services.Configure<Servers>(Configuration.GetSection("Servers"));
                services.Configure<ReCaptchaConfig>(Configuration.GetSection("ReCaptcha"));
            }

            services.AddSingleton<HttpClientSingleton>();
            services.AddScoped<AudiosRepository>();
            services.AddScoped<SquadInvitationsRepository>();
            services.AddScoped<SquadsRepository>();
            services.AddScoped<SquadJoinRequestsRepository>();
            services.AddScoped<SocketIoServiceKeysRepository>();
            services.AddScoped<KeyGenService>();
            services.AddScoped<SessionsRepository>();
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
            services.AddScoped<GoogleCloudStorageService>();
            services.AddScoped<AudiosService>();
            services.AddScoped<ImagesRepository>();
            services.AddScoped<ImagesService>();
            services.AddScoped<RecaptchaValidation>();
            // don't allow uploading files larger than 10 MB, for security reasons
            services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 1024 * 1024 * 10);
            services.Configure<JwtKeyConfig>(Configuration.GetSection("Jwt")); 
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbContextApp dbContext)
        {

            // Stop using this on production when app is for public use
            app.UseDeveloperExceptionPage();
            
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseForwardedHeaders();
            app.UseHsts();
            app.UseWebOptimizer();
            app.UseStaticFiles();
            

            app.UseRouting();
            app.UseCors("cors");
            app.UseAuthorization();
            app.UseMiddleware<ScopedHttpContextMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
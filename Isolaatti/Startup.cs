using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Isolaatti.Accounts;
using Isolaatti.Accounts.Service;
using Isolaatti.Comments.Repository;
using Isolaatti.Config;
using Isolaatti.EmailSender;
using Isolaatti.Favorites.Data;
using Isolaatti.Messaging;
using Isolaatti.Middleware;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Isolaatti.Notifications.Entity;
using Isolaatti.Notifications.PushNotifications;
using Isolaatti.Notifications.Repository;
using Isolaatti.Notifications.Services;
using Isolaatti.RealtimeInteraction.Service;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Isolaatti.Tagging;
using Isolaatti.Users.Repository;
using Isolaatti.Utils.ActionFilters;
using Isolaatti.Utils.PageFilters;
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
using MongoDB.Bson.Serialization;
using Npgsql;


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
            const string filePath = "isolaatti-firebase-adminsdk.json";
            GoogleCredential credential;
            if (File.Exists(filePath))
            {
                var file = File.Open(filePath, FileMode.Open);
                credential = GoogleCredential.FromStream(file);
                file.Close();
            }
            else
            {
                var json = Environment.GetEnvironmentVariable(Env.GoogleFirebaseAdminSdkCredential);
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
            services.AddRazorPages()
                .AddMvcOptions(options =>
                {
                    options.Filters.Add<IsolaattiAuthPagesFilter>();
                })
                .AddRazorPagesOptions(options => options.Conventions.AddPageRoute("/Profile", "/perfil/{numericId:int}"))
                .AddRazorRuntimeCompilation();
            


            services.AddDbContextPool<DbContextApp>((serviceProvider, options) =>
            {
         
                if (_environment.IsProduction())
                {
                    var databaseUrl = Environment.GetEnvironmentVariable(Env.DatabaseUrl);
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
                var mongoConfigEnvVar = Environment.GetEnvironmentVariable(Env.MongoDbConfig);
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
                    config.CommentModificationHistoryCollectionName = mongoConfig?.CommentModificationHistoryCollectionName;
                });
                
                var serversConfigEnvVar = Environment.GetEnvironmentVariable(Env.ServersConfig);
                services.Configure<Servers>(config =>
                {
                    var serversConfig = JsonSerializer.Deserialize<Servers>(serversConfigEnvVar!);
                    config.RealtimeServerUrl= serversConfig?.RealtimeServerUrl;
                    config.RealtimeServerInternalUrl = serversConfig?.RealtimeServerInternalUrl;
                });
                var recaptchaConfigEnvVar = Environment.GetEnvironmentVariable(Env.RecaptchaConfig);
                services.Configure<ReCaptchaConfig>(config =>
                {
                    var recaptchaConfig = JsonSerializer.Deserialize<ReCaptchaConfig>(recaptchaConfigEnvVar);
                    config.Site = recaptchaConfig.Site;
                    config.Secret = recaptchaConfig.Secret;
                });
                var isolaattiServicesKeysJsonEnvVar = Environment.GetEnvironmentVariable(Env.IsolaattiServiceKeys);
                services.Configure<IsolaattiServicesKeys>(config =>
                {
                    var isolaattiServiceKeys =
                        JsonSerializer.Deserialize<IsolaattiServicesKeys>(isolaattiServicesKeysJsonEnvVar);
                    config.RealtimeService = isolaattiServiceKeys.RealtimeService;
                });
                var rabbitmqConfigJsonEnvVar = Environment.GetEnvironmentVariable(Env.RabbitmqConfig);
                services.Configure<RabbitmqConfig>(config =>
                {
                    var rabbitmqConfig = JsonSerializer.Deserialize<RabbitmqConfig>(rabbitmqConfigJsonEnvVar);
                    config.Host = rabbitmqConfig.Host;
                    config.Password = rabbitmqConfig.Password;
                    config.Port = rabbitmqConfig.Port;
                    config.Username = rabbitmqConfig.Username;
                    config.VirtualHost = rabbitmqConfig.VirtualHost;
                });
                var clientsConfigJsonEnv = Environment.GetEnvironmentVariable(Env.ClientsConfig);
                services.Configure<List<Client>>(config =>
                {
                    var clientsConfig = JsonSerializer.Deserialize<List<Client>>(clientsConfigJsonEnv);
                    config.AddRange(clientsConfig);
                });
            }
            else
            {
                services.Configure<MongoDatabaseConfiguration>(Configuration.GetSection("MongoDb"));
                services.Configure<Servers>(Configuration.GetSection("Servers"));
                services.Configure<ReCaptchaConfig>(Configuration.GetSection("ReCaptcha"));
                services.Configure<IsolaattiServicesKeys>(Configuration.GetSection("IsolaattiServicesKeys"));
                services.Configure<RabbitmqConfig>(Configuration.GetSection("RabbitMQ"));
                services.Configure<List<Client>>(Configuration.GetSection("Clients"));
                services.Configure<HostConfig>(Configuration.GetSection("Host"));
            }

            services.AddSingleton<MongoDatabase>();
            services.AddSingleton<HttpClientSingleton>();
            services.AddScoped<AudiosRepository>();
            services.AddScoped<SquadInvitationsRepository>();
            services.AddScoped<SquadsRepository>();
            services.AddScoped<SquadUsersRepository>();
            services.AddScoped<SquadPermissionsRepository>();
            services.AddScoped<SquadJoinRequestsRepository>();
            services.AddScoped<NotificationsRepository>();
            services.AddScoped<NotificationsService>();
            services.AddScoped<SocketIoServiceKeysRepository>();
            services.AddScoped<KeyGenService>();
            services.AddScoped<CommentHistoryRepository>();
            services.AddSingleton<SessionsRepository>();
            services.AddDistributedMemoryCache();
            services.AddScoped<UsersRepository>();
            services.AddScoped<ScopedHttpContext>();
            services.AddScoped<IAccountsService, AccountsService>();
            services.AddScoped<NotificationSender>();
            services.AddScoped<ServerRenderedAlerts>();
            services.AddSingleton<GoogleCloudStorageService>();
            services.AddScoped<AudiosService>();
            services.AddScoped<ImagesRepository>();
            services.AddScoped<ImagesService>();
            services.AddSingleton<RecaptchaValidation>();
            services.AddSingleton<Rabbitmq>();
            services.AddSingleton<EmailSenderMessaging>();
            services.AddSingleton<PushNotificationsSenderMessaging>();
            services.AddSingleton<RegisterDeviceMessaging>();
            services.AddScoped<FavoritesRepository>();
            services.AddScoped<TaggingService>();
            services.AddScoped<AccountRemovalService>();
            
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
            
            // don't allow uploading files larger than 10 MB, for security reasons
            services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 1024 * 1024 * 10);
            
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbContextApp dbContext)
        {

            BsonClassMap.RegisterClassMap<LikeNotificationPayload>();

            // Stop using this on production when app is for public use
            app.UseDeveloperExceptionPage();
            
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseForwardedHeaders();
            app.UseHsts();
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
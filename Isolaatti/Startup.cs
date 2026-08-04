using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Audios.Service;
using Isolaatti.Auth;
using Isolaatti.Comments.Repository;
using Isolaatti.Common;
using Isolaatti.Config;
using Isolaatti.Database.EF;
using Isolaatti.Database.MongoDb;
using Isolaatti.EmailSender;
using Isolaatti.Favorites.Data;
using Isolaatti.IdentityService;
using Isolaatti.Images.Repository;
using Isolaatti.Images.Service;
using Isolaatti.Messaging;
using Isolaatti.Middleware;
using Isolaatti.Notifications.PushNotifications;
using Isolaatti.Notifications.Services;
using Isolaatti.RealtimeInteraction.Service;
using Isolaatti.Recaptcha;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Isolaatti.Squads.Repository;
using Isolaatti.Tagging;
using Isolaatti.Users.Repository;
using Isolaatti.Utils.ActionFilters;
using Isolaatti.Utils.PageFilters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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
            
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { "en-US", "es", "es-419" };
                options.SetDefaultCulture(supportedCultures[0])
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
            });
            
            services.AddRazorPages()

                .AddRazorPagesOptions(options =>
                {
                    
                    options.Conventions.AddPageRoute("/Profile", "/perfil/{numericId:int}");
                    options.Conventions.AllowAnonymousToPage("/WelcomePage");
                })
                .AddMvcOptions(options =>
                {
                    options.Filters.Add<IsolaattiAuthPagesFilter>();
                })
                .AddViewLocalization()
                .AddRazorRuntimeCompilation();
            


            services.AddDbContextPool<DbContextApp>((serviceProvider, options) =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("Database"));
            });
            services.AddDataProtection().PersistKeysToDbContext<DbContextApp>();
            
            // Configuration
            services.Configure<MongoDatabaseConfiguration>(Configuration.GetSection("MongoDb"));
            services.Configure<Servers>(Configuration.GetSection("Servers"));
            services.Configure<ReCaptchaConfig>(Configuration.GetSection("ReCaptcha"));
            services.Configure<RabbitmqConfig>(Configuration.GetSection("RabbitMQ"));
            services.Configure<List<Client>>(Configuration.GetSection("Clients"));
            services.Configure<HostConfig>(Configuration.GetSection("Host"));
            services.Configure<Keycloak>(Configuration.GetSection("Keycloak"));
            

            services.AddSingleton<MongoDatabase>();
            services.AddSingleton<HttpClientSingleton>();
            services.AddScoped<SquadsRepository>();
            services.AddScoped<SquadUsersRepository>();
            services.AddScoped<NotificationsService>();
            services.AddScoped<SocketIoServiceKeysRepository>();
            services.AddScoped<CommentHistoryRepository>();
            services.AddScoped<SessionsRepository>();
            services.AddDistributedMemoryCache();
            services.AddScoped<UsersRepository>();
            services.AddScoped<ScopedHttpContext>();
            services.AddScoped<IAccountsService, AccountsService>();
            services.AddSingleton<NotificationSender>();
            services.AddScoped<ServerRenderedAlerts>();
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
            services.AddScoped<IIdentityService, KeycloakIdentityService>();
            
            IConnectionMultiplexer redisConnection = ConnectionMultiplexer.Connect(
                Configuration.GetConnectionString("Redis")!);

            services.AddSingleton(redisConnection);
            
            services.AddStackExchangeRedisCache(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(redisConnection);
                options.InstanceName = "app:cookiesession:"; // prefijo distintivo
            });
            
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.Name = ".App.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
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

            var keycloakConfiguration = Configuration.GetSection("Keycloak").Get<Keycloak>();

            services.AddSingleton<IApiTokenStore, ApiTokenStore>();
            

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = $"{CookieAuthenticationDefaults.AuthenticationScheme}";
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.AccessDeniedPath = "/denied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                })
                .AddScheme<AuthenticationSchemeOptions, OpaqueTokenAuthenticationHandler>("OpaqueToken", options =>
                {
                })
                .AddPolicyScheme("Smart", "Smart", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                        if (authHeader != null && authHeader.StartsWith("Bearer "))
                            return "OpaqueToken";
                        return CookieAuthenticationDefaults.AuthenticationScheme;
                    };
                });
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
            app.UseStaticFiles();
            

            app.UseRouting();
            app.UseCors("cors");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<ScopedHttpContextMiddleware>();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
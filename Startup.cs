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
using isolaatti_API.Middleware;
using isolaatti_API.Models;
using isolaatti_API.Services;
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

namespace isolaatti_API
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
            services.AddSignalR();


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

            // don't allow uploading files larger than 2 MB, for security reasons
            services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 1024 * 1024 * 2);

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbContextApp dbContext,
            MyKeysDbContext keysDb)
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
            app.UseRouting();
            app.UseAuthorization();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseMiddleware<ScopedHttpContextMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IsolaattiID.Areas.Identity.Data;
using IsolaattiID.Service;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("IsolaattiIDIdentityDbContextConnection") ?? throw new InvalidOperationException("Connection string 'IsolaattiIDIdentityDbContextConnection' not found.");

builder.Services.AddDbContext<IsolaattiIDIdentityDbContext>(options => options.UseNpgsql(connectionString));

builder.Services
    .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<IsolaattiIDIdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication().AddCookie();

builder.Services.AddDataProtection().PersistKeysToDbContext<IsolaattiIDIdentityDbContext>();

// Add services to the container.
builder.Services
    .AddIdentityServer()
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddClientStore<ClientStore>()
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddDeveloperSigningCredential()
    .AddAspNetIdentity<IdentityUser>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.UseIdentityServer();

app.Run();

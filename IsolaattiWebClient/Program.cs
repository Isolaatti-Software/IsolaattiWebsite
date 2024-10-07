using Duende.Bff;
using Duende.Bff.Yarp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "oidc";
    })
    .AddCookie("Cookies", options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = false;
        options.Cookie.Name = "__Hots-spa";
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://localhost:7240";
        options.ClientId = "web";
        options.ClientSecret = "secret";
        options.ResponseMode = "query";
        options.ResponseType = "code";

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("offline_access");
        
        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.SaveTokens = true;
    });

builder.Services.AddControllersWithViews();

builder.Services.AddBff(options =>
{
    options.ManagementBasePath = "/bff";
}).AddRemoteApis();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseBff();
app.MapBffManagementEndpoints();
app.UseAuthentication();
app.UseAuthorization();


app.MapFallbackToFile("index.html").RequireAuthorization();
app.MapRemoteBffApiEndpoint("/api", "http://localhost:5000/api")
    .SkipAntiforgery()
    .RequireAccessToken(TokenType.UserOrClient);

app.Run();
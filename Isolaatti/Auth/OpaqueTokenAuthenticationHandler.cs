using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Isolaatti.Auth;

public class ApiToken
{
    public int Id { get; set; }
    public string TokenHash { get; set; } // SHA-256 del token
    public string UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string[] Scopes { get; set; }
}



public class OpaqueTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IApiTokenStore _tokenStore;
    
    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    public OpaqueTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiTokenStore tokenStore)
        : base(options, logger, encoder)
    {
        _tokenStore = tokenStore;
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
            return AuthenticateResult.NoResult();

        var token = authHeader["Bearer ".Length..].Trim();
        var tokenHash = HashToken(token);

        var apiToken = await _tokenStore.GetByHashAsync(tokenHash);
        if (apiToken == null || apiToken.ExpiresAt < DateTime.UtcNow || apiToken.RevokedAt != null)
            return AuthenticateResult.Fail("Token inválido o expirado");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, apiToken.UserId)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
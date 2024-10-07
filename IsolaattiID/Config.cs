using Duende.IdentityServer.Models;

namespace IsolaattiID.Service;

public class Config
{
    public static readonly IEnumerable<ApiScope> ApiScopes = new List<ApiScope>()
    {
        new ApiScope("endUser", "End User Access"),
        new ApiScope("adminApp", "AdminApp Access")
    };
    
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };
}
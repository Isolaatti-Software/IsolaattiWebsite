using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

namespace IsolaattiID.Service;

public class ClientStore : IClientStore
{
    private IEnumerable<Client> Clients = new[]
    {
        new Client
        {
            ClientId = "web",
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedGrantTypes = GrantTypes.Code,
            AllowOfflineAccess = true,
            AllowedScopes = { 
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Email
            },
                // where to redirect to after login
                RedirectUris = { "https://localhost:7274/signin-oidc" }, 
                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:7274/signout-callback-oidc" }
                
        }
    };
    
    public async Task<Client?> FindClientByIdAsync(string clientId)
    {
        return Clients.FirstOrDefault(client => client.ClientId == clientId);
    }
}
using Isolaatti.Config;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Isolaatti.Pages
{
    [Authorize]
    public class Settings(IOptions<Keycloak> keycloakOptions) : IsolaattiPageModel
    {
        
        public string SsoAccountAddress { get; set; } = keycloakOptions.Value.Authority + "/account";

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
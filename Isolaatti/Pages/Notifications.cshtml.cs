using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages;

[Authorize]
public class Notifications : IsolaattiPageModel
{
    private readonly IAccountsService _accounts;

    public Notifications(IAccountsService accounts)
    {
        _accounts = accounts;
    }
    
    public async Task<IActionResult> OnGet()
    {

        return Page();
    }
}
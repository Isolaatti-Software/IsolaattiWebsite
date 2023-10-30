using System.Threading.Tasks;
using Isolaatti.Accounts;
using Isolaatti.Accounts.Service;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages;

[IsolaattiAuth]
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
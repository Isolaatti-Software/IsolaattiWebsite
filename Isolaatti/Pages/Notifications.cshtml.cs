using System.Threading.Tasks;
using Isolaatti.Services;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages;

public class Notifications : IsolaattiPageModel
{
    private readonly IAccounts _accounts;

    public Notifications(IAccounts accounts)
    {
        _accounts = accounts;
    }
    
    public async Task<IActionResult> OnGet()
    {

        return Page();
    }
}
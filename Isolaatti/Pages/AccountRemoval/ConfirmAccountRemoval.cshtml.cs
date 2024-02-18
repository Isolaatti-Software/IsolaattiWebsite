using System;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages.AccountRemoval;

public class ConfirmAccountRemoval : PageModel
{
    private readonly AccountRemovalService _accountRemovalService;
    public bool Failure { get; set; }

    public ConfirmAccountRemoval(AccountRemovalService accountRemovalService)
    {
        _accountRemovalService = accountRemovalService;
    }

    public async Task<IActionResult> OnPost([FromQuery] Guid id, [FromQuery] string key)
    {
        Failure = await _accountRemovalService.ProceedWithAccountRemoval(id, key);

        if (Failure)
        {
            return Page();
        }

        return RedirectToPage("/LogIn", new
        {
            accountRemoved = true
        });
    }
}
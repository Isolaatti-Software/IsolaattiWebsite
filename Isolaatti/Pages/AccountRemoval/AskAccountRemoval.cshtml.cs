using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Helpers;
using Isolaatti.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages.AccountRemoval;

public class AskAccountRemoval : PageModel
{

    private readonly AccountRemovalService _accountRemovalService;
    public AskAccountRemoval(AccountRemovalService accountRemovalService)
    {
        _accountRemovalService = accountRemovalService;
    }

    [BindProperty]
    public string Email { get; set; }
    public bool EmailInvalid { get; set; }
    public bool EmailSent { get; set; }
    
    public async Task<IActionResult> OnPost()
    {
        if (Email.IsNullOrWhiteSpace() || !new EmailAddressAttribute().IsValid(Email))
        {
            EmailInvalid = true;
            return Page();
        }
        EmailSent = true;
        await _accountRemovalService.SendEmail(Email);
        return Page();
    }
}
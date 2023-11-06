using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages;

public class ContinueAccountCreation : PageModel
{
    private readonly IDataProtector _dataProtector;
    private readonly IAccountsService _accounts;


    public ContinueAccountCreation(IDataProtectionProvider dataProtectionProvider, IAccountsService accounts)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("codes");
        _accounts = accounts;
    }
    
    [BindProperty] public string Code { get; set; }
    [BindProperty] public bool CodeIsInvalid { get; set; }
    public void OnGet()
    {
        CodeIsInvalid = false;
    }

    public async Task<IActionResult> OnPost()
    {

        if (await _accounts.ValidatePreCreateCode(Code) != null)
        {
            
            return RedirectToPage("/ConfirmAccount", new { state =  _dataProtector.Protect(Code)});
        }

        CodeIsInvalid = true;
        return Page();

    }
}
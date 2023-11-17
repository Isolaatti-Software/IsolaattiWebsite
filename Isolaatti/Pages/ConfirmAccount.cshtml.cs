using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Enums;
using Isolaatti.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages;

public class ConfirmAccount : PageModel
{
    private readonly IAccountsService _accounts;
    private readonly RecaptchaValidation _recaptchaValidation;
    private readonly IDataProtector _dataProtector;

    public ConfirmAccount(IAccountsService accounts, RecaptchaValidation recaptchaValidation, IDataProtectionProvider dataProtectionProvider)
    {
        _accounts = accounts;
        _recaptchaValidation = recaptchaValidation;
        _dataProtector = dataProtectionProvider.CreateProtector("codes");
    }
    
    [BindProperty]
    public string State { get; set; }
    
    [BindProperty]
    public string Username { get; set; }
    [BindProperty]
    public string Name { get; set; }
    [BindProperty]
    public string Password { get; set; }
    [BindProperty]
    public string PasswordConfirmation { get; set; }
    
    [BindProperty]
    public bool RecaptchaError { get; set; }
    
    public bool MalformedUrlError { get; set; }
    public bool UnavailableUsername { get; set; }
    public string? TraceId { get; set; }

    public async Task<IActionResult> OnGet([FromQuery] string state)
    {
        if (state == null)
        {
            return NotFound();
        }
        State = state;
        return Page();
    }

    public async Task<IActionResult> OnPost([FromForm(Name = "g-recaptcha-response")] string recaptchaResponse)
    {
        if (!await _recaptchaValidation.ValidateRecaptcha(recaptchaResponse))
        {
            RecaptchaError = true;
            return Page();
        }



        try
        {
            var code = _dataProtector.Unprotect(State);
            var preCreate = await _accounts.ValidatePreCreateCode(code);
            if (preCreate == null)
            {
                MalformedUrlError = true;
                return Page();
            }
            
            var accountMakeResult = await _accounts.MakeAccountAsync(Username, Name, preCreate.Email,Password);

            switch (accountMakeResult.AccountMakingResult)
            {
                case AccountMakingResult.EmailNotAvailable:
                    break;
                case AccountMakingResult.ValidationProblems:
                    break;
                case AccountMakingResult.Ok:
                    return RedirectToPage("/LogIn", new { newUser = "true"});
                    break;
                case AccountMakingResult.Error:
                    TraceId = HttpContext.TraceIdentifier;
                    return Page();
                case AccountMakingResult.UsernameUnavailable:
                    UnavailableUsername = true;
                    return Page();
                default: return Page();
            }

        }
        catch (CryptographicException)
        {
            MalformedUrlError = true;
        }

        return Page();
    }
    
}
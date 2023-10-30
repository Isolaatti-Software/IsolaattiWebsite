using Isolaatti.Accounts.Service;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages;

public class ConfirmAccount : PageModel
{
    private readonly IAccountsService _accounts;
    private readonly RecaptchaValidation _recaptchaValidation;

    public ConfirmAccount(IAccountsService accounts, RecaptchaValidation recaptchaValidation)
    {
        _accounts = accounts;
        _recaptchaValidation = recaptchaValidation;
    }
    
    [BindProperty]
    public string Username { get; set; }
    [BindProperty]
    public string Name { get; set; }
    [BindProperty]
    public string Email { get; set; }
    [BindProperty]
    public string Password { get; set; }
    [BindProperty]
    public string PasswordConfirmation { get; set; }
        
    [BindProperty]
    public bool EmailPrevUsed { get; set; }
    [BindProperty]
    public bool RecaptchaError { get; set; }
    [BindProperty]
    public bool UsernameUnavailable { get; set; }
}
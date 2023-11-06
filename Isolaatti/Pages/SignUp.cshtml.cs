using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages
{
    public class SignUp : PageModel
    {
        private readonly RecaptchaValidation _recaptchaValidation;
        private readonly IAccountsService _accounts;
        public SignUp(IAccountsService accounts, RecaptchaValidation recaptchaValidation)
        {
            _recaptchaValidation = recaptchaValidation;
            _accounts = accounts;
        }
        
        [BindProperty] public string Email { get; set; }
        
        public bool RecaptchaError { get; set; }
        public IAccountsService.AccountPrecreateResult Result { get; set; }
        public bool Posted { get; set; }

        public void OnGet()
        {
            Posted = false;
        }
        
        public async Task<IActionResult> OnPost([FromForm(Name = "g-recaptcha-response")] string recaptchaResponse)
        {
            Posted = true;
            if (!await _recaptchaValidation.ValidateRecaptcha(recaptchaResponse))
            {
                RecaptchaError = true;
                return Page();
            }
            
            
            Result = await _accounts.PreCreateAccount(Email);

            if (Result == IAccountsService.AccountPrecreateResult.Success)
            {
                return RedirectToPage("/ContinueAccountCreation");
            }

            return Page();
        }
    }
}
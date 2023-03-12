using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Isolaatti.Config;
using Isolaatti.DTOs;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Isolaatti.Pages
{
    public class MakeAccount : PageModel
    {
        private readonly IAccounts _accounts;
        private readonly RecaptchaValidation _recaptchaValidation;

        public MakeAccount(IAccounts accounts, RecaptchaValidation recaptchaValidation)
        {
            _accounts = accounts;
            _recaptchaValidation = recaptchaValidation;
        }
        
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

        public async Task<IActionResult> OnGet(string then = "")
        {
            ViewData["alerts"] = new Dictionary<string, string>();
            if (EmailPrevUsed)
            {
                ((Dictionary<string, string>)ViewData["alerts"])["error"] =
                    "La dirección de correo introducida ya ha sido utilizada. Intenta con otra.";
            }
            

            ViewData["then"] = then;

            return Page();
        }

        public async Task<IActionResult> OnPost([FromForm(Name = "g-recaptcha-response")] string recaptchaResponse, [FromQuery] string then = "")
        {
            if (!(await _recaptchaValidation.ValidateRecaptcha(recaptchaResponse)))
            {
                RecaptchaError = true;
                return Page();
            }
            
            if (Name == null || Email == null || Password == null || PasswordConfirmation == null)
            {
                return Page();
            }

            var result = await _accounts.MakeAccountAsync(Name, Email, Password);
            switch (result)
            {
                case AccountMakingResult.Ok:
                    if (!then.Equals(""))
                    {
                        return Redirect(then);
                    }

                    return RedirectToPage("LogIn", new
                    {
                        newUser = true,
                        username = Email
                    });
                case AccountMakingResult.EmailNotAvailable:
                    EmailPrevUsed = true;
                    return Page();
                default:
                    return Page();
            }
        }
    }
}
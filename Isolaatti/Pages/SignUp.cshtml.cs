using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Isolaatti.Accounts;
using Isolaatti.Accounts.Service;
using Isolaatti.Config;
using Isolaatti.DTOs;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Services;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

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
        
        public async Task<IActionResult> OnPost([FromForm(Name = "g-recaptcha-response")] string recaptchaResponse)
        {
            if (!await _recaptchaValidation.ValidateRecaptcha(recaptchaResponse))
            {
                RecaptchaError = true;
                return Page();
            }
            
            
            Result = await _accounts.PreCreateAccount(Email);

            return Page();
        }
    }
}
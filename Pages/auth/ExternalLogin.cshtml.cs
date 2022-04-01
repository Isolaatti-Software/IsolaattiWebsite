using System;
using System.Threading.Tasks;
using System.Web;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.auth
{
    public class ExternalLogin : PageModel
    {
        private readonly DbContextApp _db;
        public string HostToLink;
        public bool MalformedUrl;
        public bool IsNotSecure;
        public bool IncorrectPassword;

        public ExternalLogin(DbContextApp db)
        {
            _db = db;
        }

        [BindProperty] public string Password { get; set; }

        public async Task<IActionResult> OnGet(string canonicalUrl = "", string tokenParamName = "")
        {
            var accountsManager = new Accounts(_db);
            var token = Request.Cookies["isolaatti_user_session_token"];
            var user = await accountsManager.ValidateToken(token);
            if (user == null) return RedirectToPage("/LogIn", new { then = Request.GetEncodedUrl() });

            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
            ViewData["profilePicUrl"] = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);

            try
            {
                var url = new Uri(canonicalUrl);
                IsNotSecure = !url.Scheme.Equals("https");
                HostToLink = url.Host;
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is UriFormatException)
            {
                MalformedUrl = true;
            }

            return Page();
        }

        public async Task<IActionResult> OnPost([FromQuery] string canonicalUrl = "",
            [FromQuery] string tokenParamName = "")
        {
            var accountsManager = new Accounts(_db);
            var token = Request.Cookies["isolaatti_user_session_token"];
            var user = await accountsManager.ValidateToken(token);
            if (user == null) return RedirectToPage("/LogIn", new { then = Request.GetEncodedUrl() });

            // I must check if the password is correct, because the confirmation button still could be clicked by a robot
            var passwordHasher = new PasswordHasher<string>();
            try
            {
                var url = new Uri(canonicalUrl);
                IsNotSecure = !url.Scheme.Equals("https");
                HostToLink = url.Host;
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is UriFormatException)
            {
                MalformedUrl = true;
            }

            if (Password == null)
            {
                IncorrectPassword = true;
                return Page();
            }

            var verificationResult = passwordHasher.VerifyHashedPassword(user.Name, user.Password, Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                IncorrectPassword = true;
                return Page();
            }


            if (canonicalUrl.Length == 0 || tokenParamName.Length == 0)
            {
                return NotFound();
            }

            return Redirect($"{canonicalUrl}?{tokenParamName}={HttpUtility.UrlEncode(token)}");
        }
    }
}
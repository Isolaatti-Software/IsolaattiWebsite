using System;
using System.Threading.Tasks;
using System.Web;
using isolaatti_API.Models;
using isolaatti_API.Services;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.auth
{
    public class ExternalLogin : PageModel
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public string HostToLink;
        public bool MalformedUrl;
        public bool IsNotSecure;
        public bool IncorrectPassword;

        public ExternalLogin(DbContextApp db, IAccounts accounts)
        {
            _db = db;
            _accounts = accounts;
        }

        public async Task<IActionResult> OnGet(string canonicalUrl = "", string tokenParamName = "")
        {
            var token = Request.Cookies["isolaatti_user_session_token"];
            var user = await _accounts.ValidateToken(token);
            if (user == null) return RedirectToPage("/LogIn", new { then = Request.GetEncodedUrl() });

            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
            ViewData["profilePicUrl"] = user.ProfileImageId == null
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
            var token = Request.Cookies["isolaatti_user_session_token"];
            var user = await _accounts.ValidateToken(token);
            if (user == null) return RedirectToPage("/LogIn", new { then = Request.GetEncodedUrl() });

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

            if (canonicalUrl.Length == 0 || tokenParamName.Length == 0)
            {
                return NotFound();
            }

            return Redirect($"{canonicalUrl}?{tokenParamName}={HttpUtility.UrlEncode(token)}");
        }
    }
}
using System;
using System.Threading.Tasks;
using System.Web;
using Isolaatti.Models;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages.auth
{
    [IsolaattiAuth]
    public class ExternalLogin : IsolaattiPageModel
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

        public IActionResult OnGet(string canonicalUrl = "", string tokenParamName = "")
        {
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
            try
            {
                var url = new Uri(canonicalUrl);
                IsNotSecure = !url.Scheme.Equals("https");
                HostToLink = url.Host;
            }
            catch (Exception ex) when (ex is ArgumentNullException or UriFormatException)
            {
                MalformedUrl = true;
            }

            if (canonicalUrl.Length == 0 || tokenParamName.Length == 0)
            {
                return NotFound();
            }

            var token = (string)ViewData["token"];
            return Redirect($"{canonicalUrl}?{tokenParamName}={HttpUtility.UrlEncode(token)}");
        }
    }
}
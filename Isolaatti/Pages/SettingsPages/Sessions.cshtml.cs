using System.Collections.Generic;
using Isolaatti.Classes.Authentication;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages.SettingsPages
{
    [IsolaattiAuth]
    public class Sessions : IsolaattiPageModel
    {
        private readonly IAccounts _accounts;
        public IEnumerable<Models.MongoDB.Session> SessionTokens;
        public AuthenticationTokenSerializable CurrentToken;

        public Sessions(IAccounts accounts)
        {
            _accounts = accounts;
        }

        public IActionResult OnGet()
        {
            CurrentToken = AuthenticationTokenSerializable.FromString(Request.Cookies["isolaatti_user_session_token"]);
            SessionTokens = _accounts.GetSessionsOfUser(User.Id);

            return Page();
        }
    }
}
using System.Collections.Generic;
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

        public Sessions(IAccounts accounts)
        {
            _accounts = accounts;
        }

        public IActionResult OnGet()
        {
            SessionTokens = _accounts.GetSessionsOfUser(User.Id);

            return Page();
        }
    }
}
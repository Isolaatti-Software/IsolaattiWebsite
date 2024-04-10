using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Database.MongoDb;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages.SettingsPages
{
    [IsolaattiAuth]
    public class Sessions : IsolaattiPageModel
    {
        private readonly IAccountsService _accounts;
        public IEnumerable<Session> SessionTokens;

        public Sessions(IAccountsService accounts)
        {
            _accounts = accounts;
        }

        public async Task<IActionResult> OnGet()
        {
            SessionTokens = await _accounts.GetSessionsOfUser(User.Id);

            return Page();
        }
    }
}
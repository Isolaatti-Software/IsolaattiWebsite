using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages
{
    [IsolaattiAuth]
    public class PrivacySettings : IsolaattiPageModel
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public PrivacySettings(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        [BindProperty] public bool ShowEmail { get; set; }
        [BindProperty] public bool PreferencesUpdated { get; set; }

        public IActionResult OnGet()
        {
            ShowEmail = User.ShowEmail;
            return Page();
        }


        public async Task<IActionResult> OnPostEmailPrivacy()
        {
            User.ShowEmail = ShowEmail;
            _db.Users.Update(User);
            await _db.SaveChangesAsync();
            PreferencesUpdated = true;
            return Page();
        }
    }
}
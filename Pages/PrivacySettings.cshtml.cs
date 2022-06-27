using System.Threading.Tasks;
using isolaatti_API.Models;
using isolaatti_API.Services;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class PrivacySettings : PageModel
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public PrivacySettings(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        [BindProperty] public bool ShowEmail { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");

            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
            ViewData["profilePicUrl"] = user.ProfileImageId == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);

            ViewData["curentSessionToken"] = Request.Cookies["isolaatti_user_session_token"];

            ShowEmail = user.ShowEmail;

            return Page();
        }


        public async Task<IActionResult> OnPostEmailPrivacy()
        {
            var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");

            user.ShowEmail = ShowEmail;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return RedirectToPage("/PrivacySettings");
        }
    }
}
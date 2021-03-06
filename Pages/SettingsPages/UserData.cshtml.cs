using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Services;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages
{
    public class UserData : PageModel
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public User IsolaattiUser;
        public List<string> AudioUrls;

        public UserData(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");
            IsolaattiUser = user;

            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
            ViewData["profilePicUrl"] = user.ProfileImageId == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);

            ViewData["curentSessionToken"] = Request.Cookies["isolaatti_user_session_token"];

            AudioUrls = (
                from post in _db.SimpleTextPosts
                where post.UserId.Equals(user.Id) && post.AudioAttachedUrl != null
                select post.AudioAttachedUrl).ToList();

            AudioUrls.AddRange((from comment in _db.Comments
                where comment.WhoWrote.Equals(user.Id) && comment.AudioUrl != null
                select comment.AudioUrl).ToList());

            return Page();
        }
    }
}
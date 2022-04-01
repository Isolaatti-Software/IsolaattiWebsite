using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class UserData : PageModel
    {
        private readonly DbContextApp _db;
        public User IsolaattiUser;
        public List<string> AudioUrls;

        public UserData(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        public async Task<IActionResult> OnGet()
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");
            IsolaattiUser = user;

            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
            ViewData["profilePicUrl"] = user.ProfileImageData == null
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
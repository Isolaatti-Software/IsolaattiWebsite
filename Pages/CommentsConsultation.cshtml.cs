using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class CommentsConsultation : PageModel
    {
        private readonly DbContextApp _db;
        public Comment[] Comments;

        public CommentsConsultation(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        public IActionResult OnGet()
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");

            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
            ViewData["profilePicUrl"] = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);

            ViewData["curentSessionToken"] = Request.Cookies["isolaatti_user_session_token"];

            Comments = _db.Comments.Where(comment => comment.WhoWrote.Equals(user.Id)).ToArray();

            return Page();
        }
    }
}
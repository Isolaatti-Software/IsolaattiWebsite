using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Services;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages
{
    public class Threads : PageModel
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public SimpleTextPost ThisPost;

        public Threads(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        public async Task<IActionResult> OnGet([FromRoute] long id)
        {
            var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            ThisPost = await _db.SimpleTextPosts.FindAsync(id);

            if (ThisPost == null) return NotFound();

            switch (user)
            {
                case null when ThisPost.Privacy != 3:
                    var protocol = Request.IsHttps ? "https://" : "http://";
                    var url = $"{protocol}{Request.HttpContext.Request.Host.Value}";
                    url += Request.Path;
                    return RedirectToPage("LogIn", new
                    {
                        then = url
                    });
                case null when ThisPost.Privacy == 3:
                    return RedirectToPage("/PublicContent/PublicThreadViewer", new
                    {
                        id = ThisPost.Id
                    });
            }

            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["profilePicUrl"] = user.ProfileImageId == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);
            return Page();
        }
    }
}
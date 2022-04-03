using System.Threading.Tasks;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Threads : PageModel
    {
        private readonly DbContextApp _db;
        public SimpleTextPost ThisPost;

        public Threads(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        public async Task<IActionResult> OnGet([FromRoute] long id)
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
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
            ViewData["profilePicUrl"] = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);
            return Page();
        }
    }
}
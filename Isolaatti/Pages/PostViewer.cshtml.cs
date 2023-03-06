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
        public long PostId;
        
        public Threads(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        public async Task<IActionResult> OnGet([FromRoute] long id)
        {
            var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            var post = await _db.SimpleTextPosts.FindAsync(id);
            
            
            if (post == null) return NotFound();
            PostId = post.Id;
            
            if (user == null)
            {
                return RedirectToPage("LogIn", new
                {
                    then = Request.Path
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
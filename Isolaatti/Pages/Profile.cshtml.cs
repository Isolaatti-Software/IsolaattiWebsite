using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.Models;
using Isolaatti.Services;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Pages
{
    public class Profile : PageModel
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;
        
        public Profile(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        public async Task<IActionResult> OnGet(int id, [FromQuery] bool noRedirect = false)
        {
            var token = Request.Cookies["isolaatti_user_session_token"];
            var user = await _accounts.ValidateToken(token);
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
            ViewData["no-redirect"] = noRedirect;
            // get profile with id
            var profile = _db.Users.Find(id);
            if (profile == null) return NotFound();
            if (profile.Id == user.Id && !noRedirect) return RedirectToPage("MyProfile");
            ViewData["profile_id"] = profile.Id;
            ViewData["profilePicUrl"] = user.ProfileImageId == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);
            ViewData["profile_name"] = profile.Name;
            
            return Page();
        }
    }
}
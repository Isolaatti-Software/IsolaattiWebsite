using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Profile : PageModel
    {
        private readonly DbContextApp _db;
        
        public Profile(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        public IActionResult OnGet([FromQuery] int id)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");
            
            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
                    
            // get profile with id
            var profile = _db.Users.Find(id);
            if (profile == null) return NotFound();
            if (profile.Id == user.Id) return RedirectToPage("MyProfile");
            ViewData["profile_name"] = profile.Name;
            ViewData["profile_email"] = profile.Email;
            ViewData["profile_id"] = profile.Id;
            if (user.Id == profile.Id) return RedirectToPage("MyProfile");

            var followingUsersIds = JsonSerializer.Deserialize<List<int>>(profile.FollowingIdsJson);
            var followersIds = JsonSerializer.Deserialize<List<int>>(profile.FollowersIdsJson);
            ViewData["numberOfLikes"] = _db.Likes.Count(like => like.TargetUserId.Equals(profile.Id));
            ViewData["followingThisUser"] = followersIds.Contains(user.Id);
            ViewData["thisUserIsFollowingMe"] = followingUsersIds.Contains(user.Id);
            ViewData["numberOfFollowers"] = followersIds.Count();
            ViewData["numberOfFollowing"] = followingUsersIds.Count();
                    
            ViewData["numberOfPosts"] = _db.SimpleTextPosts.Count(post => post.UserId.Equals(profile.Id));
            return Page();
        }
    }
}
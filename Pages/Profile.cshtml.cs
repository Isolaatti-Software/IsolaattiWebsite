using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Profile : PageModel
    {
        private readonly DbContextApp _db;
        public string ProfilePhotoUrl = null;
        public string SessionToken;

        public Profile(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        public IActionResult OnGet([FromQuery] int id)
        {
            var token = Request.Cookies["isolaatti_user_session_token"];
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(token);
            if (user == null) return RedirectToPage("/PublicContent/Profile", new { id = id });

            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;

            // get profile with id
            var profile = _db.Users.Find(id);
            if (profile == null) return NotFound();
            if (profile.Id == user.Id) return RedirectToPage("MyProfile");
            ViewData["profile_name"] = profile.Name;
            ViewData["profile_email"] = profile.Email;
            ViewData["profile_id"] = profile.Id;
            if (user.Id == profile.Id) return RedirectToPage("MyProfile");
            ViewData["profilePicUrl"] = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);

            ViewData["numberOfLikes"] = _db.Likes.Count(like => like.TargetUserId.Equals(profile.Id));
            ViewData["followingThisUser"] =
                _db.FollowerRelations.Any(rel => rel.UserId.Equals(user.Id) && rel.TargetUserId.Equals(profile.Id));
            ViewData["thisUserIsFollowingMe"] = _db.FollowerRelations.Any(rel =>
                rel.UserId.Equals(profile.Id) && rel.TargetUserId.Equals(user.Id));
            ViewData["numberOfFollowers"] = profile.NumberOfFollowers;
            ViewData["numberOfFollowing"] = profile.NumberOfFollowing;
            ViewData["description"] = profile.DescriptionText;

            ViewData["numberOfPosts"] = _db.SimpleTextPosts.Count(post => post.UserId.Equals(profile.Id));

            ProfilePhotoUrl = UrlGenerators.GenerateProfilePictureUrl(profile.Id, token, Request);

            ViewData["audioDescription"] = profile.DescriptionAudioUrl;

            return Page();
        }
    }
}
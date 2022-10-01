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

        public string ProfilePhotoUrl = null;
        public string SessionToken;
        public string ProfileColor;
        public List<User> Followers = new List<User>();
        public List<User> Following = new List<User>();

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
            ViewData["profile_name"] = profile.Name;
            ViewData["profile_email"] = profile.Email;
            ViewData["profile_id"] = profile.Id;
            ViewData["profilePicUrl"] = user.ProfileImageId == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);

            ViewData["numberOfLikes"] = await _db.Likes.CountAsync(like => like.TargetUserId.Equals(profile.Id));
            ViewData["numberOfLikesGiven"] = await _db.Likes.CountAsync(like => like.UserId.Equals(profile.Id));
            ViewData["numberOfComments"] =
                await _db.Comments.CountAsync(comment => comment.TargetUser.Equals(profile.Id));
            ViewData["numberOfCommentsGiven"] =
                await _db.Comments.CountAsync(comment => comment.WhoWrote.Equals(profile.Id));

            ViewData["followingThisUser"] =
                _db.FollowerRelations.Any(rel => rel.UserId.Equals(user.Id) && rel.TargetUserId.Equals(profile.Id));
            ViewData["thisUserIsFollowingMe"] = await _db.FollowerRelations.AnyAsync(rel =>
                rel.UserId.Equals(profile.Id) && rel.TargetUserId.Equals(user.Id));
            ViewData["numberOfFollowers"] = profile.NumberOfFollowers;
            ViewData["numberOfFollowing"] = profile.NumberOfFollowing;
            ViewData["description"] = profile.DescriptionText;

            ViewData["numberOfPosts"] = await _db.SimpleTextPosts.CountAsync(post => post.UserId.Equals(profile.Id));

            ProfilePhotoUrl = UrlGenerators.GenerateProfilePictureUrl(profile.Id, token, Request);

            ViewData["showEmail"] = profile.ShowEmail;

            Followers = (
                from _user in _db.Users
                from relation in _db.FollowerRelations
                where relation.TargetUserId == profile.Id && relation.UserId == _user.Id
                select _user).ToList();
            Following = (
                from _user in _db.Users
                from relation in _db.FollowerRelations
                where relation.UserId == profile.Id && relation.TargetUserId == _user.Id
                select _user).ToList();

            try
            {
                var color = JsonSerializer.Deserialize<UserPreferences>(profile.UserPreferencesJson).ProfileHtmlColor;
                ProfileColor = color ?? "#731D8C";
            }
            catch (JsonException)
            {
                ProfileColor = "#731D8C";
            }

            return Page();
        }
    }
}
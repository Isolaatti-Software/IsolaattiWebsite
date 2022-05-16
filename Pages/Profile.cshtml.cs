using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace isolaatti_API.Pages
{
    public class Profile : PageModel
    {
        private readonly DbContextApp _db;
        public string ProfilePhotoUrl = null;
        public string SessionToken;
        public string ProfileColor;
        public List<User> Followers = new List<User>();
        public List<User> Following = new List<User>();

        public Profile(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        public async Task<IActionResult> OnGet(int id)
        {
            var token = Request.Cookies["isolaatti_user_session_token"];
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(token);
            if (user == null)
            {
                var protocol = Request.IsHttps ? "https://" : "http://";
                var url = $"{protocol}{Request.HttpContext.Request.Host.Value}";
                url += Request.Path;
                return RedirectToPage("LogIn", new
                {
                    then = url
                });
            }

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

            ViewData["audioDescription"] = profile.DescriptionAudioUrl;
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
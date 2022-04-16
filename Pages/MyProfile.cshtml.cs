/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System.Collections.Generic;
using System.IO;
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
    public class MyProfile : PageModel
    {
        private readonly DbContextApp _db;

        public bool PasswordIsWrong = false;
        public List<ShareLink> Shares = new List<ShareLink>();
        public List<User> Followers = new List<User>();
        public List<User> Following = new List<User>();
        public string ProfilePhotoUrl = null;
        public string ProfileColor;

        public MyProfile(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        public async Task<IActionResult> OnGet(
            string open = "",
            bool currentPasswordIsWrong = false,
            bool profileUpdate = false,
            bool nameAndEmailUsed = false,
            bool nameNotAvailable = false,
            bool emailNotAvailable = false,
            string statusData = "")
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
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
            ViewData["password"] = user.Password;
            ViewData["id"] = user.Id;
            ViewData["profile_open"] = open;
            ViewData["profile_updated"] = profileUpdate;
            ViewData["emailNotAvailable"] = emailNotAvailable;
            ViewData["nameNotAvailable"] = nameNotAvailable;
            ViewData["statusData"] = statusData;
            ViewData["profilePicUrl"] = user.ProfileImageId == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);
            ViewData["description"] = user.DescriptionText;
            ViewData["audioDescriptionUrl"] = user.DescriptionAudioUrl;
            PasswordIsWrong = currentPasswordIsWrong;

            ViewData["numberOfLikes"] = await _db.Likes.CountAsync(like => like.TargetUserId.Equals(user.Id));
            ViewData["numberOfPosts"] = await _db.SimpleTextPosts.CountAsync(post => post.UserId.Equals(user.Id));
            ProfilePhotoUrl =
                UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"],
                    Request);

            ViewData["sessionToken"] = Request.Cookies["isolaatti_user_session_token"];

            Followers = await (
                from _user in _db.Users
                from relation in _db.FollowerRelations
                where relation.TargetUserId == user.Id && relation.UserId == _user.Id
                select _user).ToListAsync();
            Following = await (
                from _user in _db.Users
                from relation in _db.FollowerRelations
                where relation.UserId == user.Id && relation.TargetUserId == _user.Id
                select _user).ToListAsync();

            ViewData["numberOfFollowers"] = user.NumberOfFollowers;
            ViewData["numberOfFollowing"] = user.NumberOfFollowing;

            try
            {
                var color = (await JsonSerializer.DeserializeAsync<UserPreferences>(
                    new MemoryStream(System.Text.Encoding.UTF8.GetBytes(user.UserPreferencesJson)))).ProfileHtmlColor;
                ProfileColor = color ?? "#731D8C";
            }
            catch (JsonException)
            {
                ProfileColor = "#731D8C";
            }

            return Page();
        }

        public async Task<IActionResult> OnPost(int userId, string current_password, string new_password)
        {
            if (current_password == null || new_password == null)
            {
                return RedirectToPage("MyProfile", new
                {
                    errorChangingPass = true
                });
            }

            var accountsManager = new Accounts(_db);

            if (!await accountsManager.ChangeAPassword(userId, current_password, new_password))
            {
                return RedirectToPage("MyProfile", new
                {
                    currentPasswordIsWrong = true
                });
            }

            await accountsManager.RemoveAllUsersTokens(userId);
            return Redirect("WebLogOut");
        }
    }
}
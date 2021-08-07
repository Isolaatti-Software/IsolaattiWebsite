/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        public MyProfile(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        public IActionResult OnGet(
            string open = "",
            bool currentPasswordIsWrong = false,
            bool profileUpdate = false,
            bool nameAndEmailUsed = false,
            bool nameNotAvailable = false,
            bool emailNotAvailable = false,
            string statusData = "")
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");
            
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
            ProfilePhotoUrl = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);
            ViewData["description"] = user.DescriptionText;


            PasswordIsWrong = currentPasswordIsWrong;
            var shares =
                from share in _db.SharedSongs
                join song in _db.Songs on share.userId equals song.OwnerId
                where share.SharedSongId == song.Id && share.userId == user.Id
                select new
                {
                    Name = song.OriginalFileName,
                    Artist = song.Artist,
                    Uid = share.uid,
                    Url = $"https://{Request.HttpContext.Request.Host.Value}/PublicContent/Shared?uid={share.uid}"
                };

            foreach (var share in shares)
            {
                this.Shares.Add(CastAnonymousObjectIntoShareLink(share));
            }

            var followersIds = JsonSerializer.Deserialize<List<int>>(user.FollowersIdsJson);
            foreach (var followerId in followersIds)
            {
                Followers.Add(_db.Users.Find(followerId));
            }

            var followingIds = JsonSerializer.Deserialize<List<int>>(user.FollowingIdsJson);
            foreach (var followingId in followingIds)
            {
                Following.Add(_db.Users.Find(followingId));
            }

            ViewData["numberOfFollowers"] = user.NumberOfFollowers;
            ViewData["numberOfFollowing"] = user.NumberOfFollowing;
            ViewData["numberOfLikes"] = _db.Likes.Count(like => like.TargetUserId.Equals(user.Id));
            ViewData["numberOfPosts"] = _db.SimpleTextPosts.Count(post => post.UserId.Equals(user.Id));
            ViewData["profilePicUrl"] = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);

            ViewData["sessionToken"] = Request.Cookies["isolaatti_user_session_token"];

            return Page();
        }
        public IActionResult OnPost(int userId, string current_password, string new_password)
        {
            if (current_password == null || new_password == null)
            {
                return RedirectToPage("MyProfile", new
                {
                    errorChangingPass = true
                });
            }

            var accountsManager = new Accounts(_db);

            if (!accountsManager.ChangeAPassword(userId, current_password, new_password))
            {
                return RedirectToPage("MyProfile", new
                {
                    currentPasswordIsWrong = true
                });
            }
            
            accountsManager.RemoveAllUsersTokens(userId);
            return Redirect("WebLogOut");
        }
        private ShareLink CastAnonymousObjectIntoShareLink(Object queryObject)
        {
            // properties information
            PropertyInfo propertyInfoName = queryObject.GetType().GetProperty("Name");
            PropertyInfo propertyInfoArtist = queryObject.GetType().GetProperty("Artist");
            PropertyInfo propertyInfoUid = queryObject.GetType().GetProperty("Uid");
            PropertyInfo propertyInfoUrl = queryObject.GetType().GetProperty("Url");

            // get values from properties casting from property information
            return new ShareLink()
            {
                Name = (String) (propertyInfoName.GetValue(queryObject, null)),
                Artist = (String) (propertyInfoArtist.GetValue(queryObject, null)),
                Uid = (String) (propertyInfoUid.GetValue(queryObject, null)),
                Url = (String) (propertyInfoUrl.GetValue(queryObject, null))
            };
        }
    }
}
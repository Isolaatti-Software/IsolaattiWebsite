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
            ViewData["profilePicUrl"] = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);
            ViewData["description"] = user.DescriptionText;
            PasswordIsWrong = currentPasswordIsWrong;
            

            var followersIds = JsonSerializer.Deserialize<List<Guid>>(user.FollowersIdsJson);
            List<Guid> invalidFollowers = null;
            
            foreach (var followerId in followersIds)
            {
                var follower = _db.Users.Find(followerId);
                if (follower == null)
                {
                    invalidFollowers ??= new List<Guid>();
                    invalidFollowers.Add(followerId);
                }
                else
                {
                    Followers.Add(follower);
                }
            }

            var followingIds = JsonSerializer.Deserialize<List<Guid>>(user.FollowingIdsJson);
            List<Guid> invalidFollowing = null;
            foreach (var followingId in followingIds)
            {
                var following = _db.Users.Find(followingId);
                if (following == null)
                {
                    invalidFollowing ??= new List<Guid>();
                    invalidFollowing.Add(followingId);
                }
                else
                {
                    Following.Add(following);
                }
            }

            
            ViewData["numberOfLikes"] = _db.Likes.Count(like => like.TargetUserId.Equals(user.Id));
            ViewData["numberOfPosts"] = _db.SimpleTextPosts.Count(post => post.UserId.Equals(user.Id));
            ProfilePhotoUrl = UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"], Request);

            ViewData["sessionToken"] = Request.Cookies["isolaatti_user_session_token"];
            
            PurgeInvalidFollowersAndFollowings(invalidFollowers,invalidFollowing,user);

            _db.SaveChanges();

            ViewData["numberOfFollowers"] = user.NumberOfFollowers;
            ViewData["numberOfFollowing"] = user.NumberOfFollowing;
            return Page();
        }
        public IActionResult OnPost(Guid userId, string current_password, string new_password)
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
        

        /*
         * This method recount user followers and following. Removes invalid followers and followings
         */
        private void PurgeInvalidFollowersAndFollowings(List<Guid> invalidFollowers, List<Guid> invalidFollowings, User user)
        {
            var modifiableUser = user;
            
            var followers = JsonSerializer.Deserialize <List<Guid>>(modifiableUser.FollowersIdsJson);
            if (invalidFollowers != null)
            {
                followers.RemoveAll(invalidFollowers.Contains);
                modifiableUser.FollowersIdsJson = JsonSerializer.Serialize(followers);
            }
            modifiableUser.NumberOfFollowers = followers.Count();
            _db.Users.Update(modifiableUser);

            var following = JsonSerializer.Deserialize<List<Guid>>(modifiableUser.FollowingIdsJson);
            if (invalidFollowings != null)
            {
                following.RemoveAll(invalidFollowings.Contains);
                modifiableUser.FollowingIdsJson = JsonSerializer.Serialize(following);
            }
            modifiableUser.NumberOfFollowing = following.Count();
            _db.Users.Update(modifiableUser);
            
        }
    }
}
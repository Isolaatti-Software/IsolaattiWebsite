using System;
using System.Linq;
using System.Security.Policy;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.PublicContent
{
    public class Profile : PageModel
    {
        private readonly DbContextApp _db;

        public Profile(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        public PublicProfile TheProfile;
        public bool ShouldShow = true;
        public string ProfilePhotoUrl = "";
        public IActionResult OnGet(Guid id)
        {
            var user = _db.Users.Find(id);
            if (user == null) return NotFound();
            var hasPublicPosts = _db.SimpleTextPosts.Any(post => post.UserId.Equals(user.Id) && post.Privacy.Equals(3));
            if (hasPublicPosts)
            {
                TheProfile = new PublicProfile()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Description = user.DescriptionText,
                    NumberOfFollowers = user.NumberOfFollowers,
                    NumberOfFollowing = user.NumberOfFollowing
                };
                ProfilePhotoUrl = Utils.UrlGenerators.GenerateProfilePictureUrl(user.Id, "", HttpContext.Request);
                ViewData["Title"] = user.Name + " - " + "Public profile";
                
                return Page();
            }

            TheProfile = null;
            ShouldShow = false;
            ViewData["Title"] = "Public profile not available";
            return Page();
        }
    }
}
using System;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class PostEditor : PageModel
    {
        private readonly DbContextApp _db;
        public bool Edit = false;
        public long EditPostId;
        private Guid zeroes;

        public PostEditor(DbContextApp dbContextApp, IWebHostEnvironment env)
        {
            _db = dbContextApp;
            zeroes = new Guid(new Byte[16]);
        }

        public IActionResult OnGet(bool edit = false, long postId = -1)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");
            Edit = edit;
            EditPostId = postId;

            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["profilePicUrl"] = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);

            if (!edit || postId == -1) return Page();

            var post = _db.SimpleTextPosts.Find(postId);
            if (post == null || post.UserId != user.Id) return NotFound();

            return Page();
        }
    }
}
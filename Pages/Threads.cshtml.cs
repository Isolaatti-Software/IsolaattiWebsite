using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Threads : PageModel
    {
        private readonly DbContextApp _db;
        public SimpleTextPost ThisPost;

        public Threads(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        public IActionResult OnGet([FromRoute] Guid id)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            ThisPost = _db.SimpleTextPosts.Find(id);
            
            if (ThisPost == null) return NotFound();
            
            if (user == null && ThisPost.Privacy != 3)
            {
                return RedirectToPage("LogIn");
            }

            if (user == null && ThisPost.Privacy == 3)
            {
                return RedirectToPage("/PublicContent/PublicThreadViewer", new
                {
                    id = ThisPost.Id
                });
            }
            
            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["profilePicUrl"] = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);
            return Page();
        }
    }
}
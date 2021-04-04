using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using isolaatti_API.Classes;
using isolaatti_API.Models;
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
        
        public IActionResult OnGet([FromRoute] long id)
        {
            var email = Request.Cookies["isolaatti_user_name"];
            var password = Request.Cookies["isolaatti_user_password"];

            if (email == null || password == null)
            {
                return RedirectToPage("LogIn");
            }

            try
            {
                var user = _db.Users.Single(user => user.Email.Equals(email));
                if (user.Password.Equals(password))
                {
                    if (!user.EmailValidated)
                        return RedirectToPage("LogIn", new
                        {
                            username = email,
                            notVerified = true
                        });
                    // here it's know that account is correct. Data binding!
                    ViewData["name"] = user.Name;
                    ViewData["email"] = user.Email;
                    ViewData["userId"] = user.Id;
                    ViewData["password"] = user.Password;

                    var followingIds = JsonSerializer.Deserialize<List<int>>(user.FollowingIdsJson);
                    
                    var followingNames = followingIds.Select(followingId => new IdToUser() {Id = followingId, Name = _db.Users.Find(followingId).Name}).ToList();

                    ViewData["followingJSON"] = JsonSerializer.Serialize(followingNames);

                    ThisPost = _db.SimpleTextPosts.Find(id);
                    if (ThisPost == null) return NotFound();

                    ViewData["thisPostAuthor"] = _db.Users.Find(ThisPost.UserId).Name;
                    return Page();
                }
            }
            catch (InvalidOperationException)
            {
                return RedirectToPage("LogIn");
            }
            
            return RedirectToPage("LogIn", new
            {
                badCredential = true,
                username = email
            });
        }
    }
}
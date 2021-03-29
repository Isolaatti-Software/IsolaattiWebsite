using System;
using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Recorder : PageModel
    {
        private readonly DbContextApp Db;
        public Song Song;

        public Recorder(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        
        public IActionResult OnGet()
        {
            return NotFound();
        }
        
        public IActionResult OnPost(int projectId, string name, float duration)
        {
            var email = Request.Cookies["isolaatti_user_name"];
            var password = Request.Cookies["isolaatti_user_password"];

            if (email == null || password == null)
            {
                return RedirectToPage("LogIn");
            }

            try
            {
                var user = Db.Users.Single(user => user.Email.Equals(email));
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

                    Song = Db.Songs.Find(projectId);
                    if (Song == null) return NotFound();

                    ViewData["track_name"] = name;
                    ViewData["duration"] = duration;
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
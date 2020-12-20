using System;
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.WebApp
{
    public class Play : PageModel
    {
        private readonly DbContextApp _db;
        public Play(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        public Song song;
        public IActionResult OnGet(int id)
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

                    try
                    {
                        // get the song requested
                        song = _db.Songs.Find(id);
                        if (song.OwnerId.Equals(user.Id))
                            return Page();

                        return StatusCode(404);
                    }
                    catch (InvalidOperationException)
                    {
                        return StatusCode(404);
                    }
                    
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
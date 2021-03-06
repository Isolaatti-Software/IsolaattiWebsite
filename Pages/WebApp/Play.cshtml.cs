/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
        public List<TrackPreferences> TrackPreferencesList;
        public IActionResult OnGet(int id, bool android=false)
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
                    

                    ViewData["isAndroid"] = android;
                    try
                    {
                        // get the song requested
                        song = _db.Songs.Find(id);
                        if (song.OwnerId.Equals(user.Id))
                        {
                            TrackPreferencesList =
                                JsonSerializer.Deserialize<List<TrackPreferences>>(song.TracksSettings);
                            ViewData["songId"] = song.Id;
                            return Page();
                        }
                        
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
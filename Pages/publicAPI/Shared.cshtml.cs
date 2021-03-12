/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.publicAPI
{
    public class Shared : PageModel
    {
        private readonly DbContextApp db;
        public bool SignedIn = false;
        public List<TrackPreferences> TrackPreferencesList;
        public Shared(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        public IActionResult OnGet(string uid)
        {
            var email = Request.Cookies["isolaatti_user_name"];
            var password = Request.Cookies["isolaatti_user_password"];

            if (email != null && password != null)
            {
                // Put on screen alert to ask user to import project only if it's signed in
                var user = db.Users.Single(user_ => user_.Email.Equals(email));
                if (user != null)
                {
                    if (user.Password.Equals(password) && user.EmailValidated)
                    {
                        SignedIn = true;
                    }  
                }
            }
            try
            {
                var songRef = db.SharedSongs.Single(song_ => song_.uid.Equals(uid));
                var song = db.Songs.Find(songRef.SharedSongId);
                var owner = db.Users.Find(song.OwnerId);
                ViewData["uid"] = songRef.uid;
                ViewData["name"] = song.OriginalFileName;
                ViewData["artist"] = song.Artist;
                ViewData["owner"] = owner.Name;
                ViewData["bass_url"] = song.BassUrl;
                ViewData["drums_url"] = song.DrumsUrl;
                ViewData["vocals_url"] = song.VoiceUrl;
                ViewData["other_url"] = song.OtherUrl;

                TrackPreferencesList = JsonSerializer.Deserialize<List<TrackPreferences>>(song.TracksSettings);
                return Page();
            }
            catch (Exception e)
            {
                return StatusCode(404);
            }
        }
    }
}
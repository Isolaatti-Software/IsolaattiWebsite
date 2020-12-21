/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.publicAPI
{
    public class Shared : PageModel
    {
        private readonly DbContextApp db;
        public Shared(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        public IActionResult OnGet(string uid)
        {
            try
            {
                var songRef = db.SharedSongs.Single(song => song.uid.Equals(uid));
                var song = db.Songs.Find(songRef.SharedSongId);
                var owner = db.Users.Find(song.OwnerId);
                ViewData["name"] = song.OriginalFileName;
                ViewData["artist"] = song.Artist;
                ViewData["owner"] = owner.Name;
                ViewData["bass_url"] = song.BassUrl;
                ViewData["drums_url"] = song.DrumsUrl;
                ViewData["vocals_url"] = song.VoiceUrl;
                ViewData["other_url"] = song.OtherUrl;
                return Page();
            }
            catch (Exception e)
            {
                return StatusCode(404);
            }
            
        }
    }
}
using System;
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.WebApp
{
    public class ImportFromShared : PageModel
    {
        private readonly DbContextApp _db;
        public ImportFromShared(DbContextApp dbContext)
        {
            _db = dbContext;
        }
        public IActionResult OnGet(string id)
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

                    var sharedSongRef = _db.SharedSongs.Single(element => element.uid.Equals(id));
                    if (sharedSongRef == null)
                    {
                        return NotFound("Page was not found!!");
                    }

                    var songRef = _db.Songs.Find(sharedSongRef.SharedSongId);
                    if (songRef == null)
                    {
                        return NotFound("Reference exists, but the song it pointed to does not exist");
                    }

                    ViewData["SongId"] = songRef.Id;
                    ViewData["SharedSongName"] = songRef.OriginalFileName;
                    ViewData["SharedSongArtist"] = songRef.Artist;
                    
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

        public IActionResult OnPost(int songId)
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

                    // this is the song we want to import
                    var songToImport = _db.Songs.Find(songId);
                    if (songToImport == null)
                    {
                        return RedirectToPage("Index");
                    }
                    
                    // so we create a new object and copy the urls and names
                    // from the other
                    var newSong = new Song()
                    {
                        OwnerId = user.Id,
                        OriginalFileName = songToImport.OriginalFileName,
                        Artist = songToImport.Artist,
                        BassUrl = songToImport.BassUrl,
                        DrumsUrl = songToImport.DrumsUrl,
                        VoiceUrl = songToImport.VoiceUrl,
                        OtherUrl = songToImport.OtherUrl,
                        EffectsDefinitionJsonArray = songToImport.EffectsDefinitionJsonArray,
                        TracksSettings = songToImport.TracksSettings
                    };
                    _db.Songs.Add(newSong);
                    _db.SaveChanges();
                    return RedirectToPage("Songs");
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
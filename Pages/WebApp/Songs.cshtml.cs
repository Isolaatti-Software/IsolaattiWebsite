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

namespace isolaatti_API.Pages.WebApp
{
    public class Songs : PageModel
    {
        private readonly DbContextApp _db;
        public IQueryable<Song> SongsList;
        public IQueryable<Song> SongsBeingProcessedList;

        public Songs(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        public IActionResult OnGet()
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
                    // possible errors if there are no songs in the database (empty table)
                    try
                    {
                        // get songs
                        SongsList = _db.Songs
                            .Where(song => song.OwnerId.Equals(user.Id) && !song.IsBeingProcessed);
                    
                        // get song that are being processed
                        SongsBeingProcessedList = _db.Songs
                            .Where(song => song.OwnerId.Equals(user.Id) && song.IsBeingProcessed);
                    }
                    catch (InvalidOperationException)
                    {
                    }
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
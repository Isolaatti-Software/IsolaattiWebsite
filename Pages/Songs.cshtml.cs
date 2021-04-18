/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Songs : PageModel
    {
        private readonly DbContextApp _db;
        public IQueryable<Song> SongsList;
        public IQueryable<Song> SongsBeingProcessedList;
        public IWebHostEnvironment Environment;

        public Songs(DbContextApp dbContextApp, IWebHostEnvironment env)
        {
            _db = dbContextApp;
            Environment = env;
        }
        public IActionResult OnGet()
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");
            
            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
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
}
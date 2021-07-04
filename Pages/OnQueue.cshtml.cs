/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using System.Collections.Generic;
using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class OnQueue : PageModel
    {
        private readonly DbContextApp _db;
        public List<SongQueue> SongsOnQueue;
        public bool EmptyQueue = false;

        public OnQueue(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
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
            ViewData["profilePicUrl"] = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);
                    
            // get songs on queue
            try
            {
                SongsOnQueue = _db.SongsQueue
                    .Where(element => element
                        .UserId.Equals(user.Id.ToString()) && !element.Reserved).ToList();
                SongsOnQueue.Reverse();
            }
            catch (InvalidOperationException)
            {
                EmptyQueue = true;
            }
            
            return Page();
        }
    }
}
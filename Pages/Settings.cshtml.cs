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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Settings : PageModel
    {
        private readonly DbContextApp _db;
        
        public Settings(DbContextApp dbContextApp)
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
            ViewData["password"] = user.Password;

            // values for settings
            ViewData["notify_by_email"] = user.NotifyByEmail;
            ViewData["notify_when_starts"] = user.NotifyWhenProcessStarted;
            ViewData["notify_when_finish"] = user.NotifyWhenProcessFinishes;

            ViewData["number_of_songs"] = _db.Songs.Count(song => song.OwnerId.Equals(user.Id));

            return Page();
        }
    }
}
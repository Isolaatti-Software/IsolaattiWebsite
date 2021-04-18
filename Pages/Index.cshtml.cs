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
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Index : PageModel
    {
        private readonly DbContextApp _db;

        public Index(DbContextApp dbContextApp, IWebHostEnvironment env)
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

            var followingIds = JsonSerializer.Deserialize<List<int>>(user.FollowingIdsJson);
            var followingNames = followingIds.Select(followingId =>
                new IdToUser() {Id = followingId, Name = _db.Users.Find(followingId).Name}).ToList();
            ViewData["followingJSON"] = JsonSerializer.Serialize(followingNames);
            
            return Page();
        }
    }
}
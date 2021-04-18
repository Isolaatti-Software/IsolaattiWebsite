using System;
using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Recorder : PageModel
    {
        private readonly DbContextApp Db;
        public Song Song;

        public Recorder(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        
        public IActionResult OnGet()
        {
            return NotFound();
        }
        
        public IActionResult OnPost(int projectId, string name, float duration)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");
            
            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;

            Song = Db.Songs.Find(projectId);
            if (Song == null) return NotFound();

            ViewData["track_name"] = name;
            ViewData["duration"] = duration;
            return Page();
        }
    }
}
using System.Collections.Generic;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class DownloadProject : PageModel
    {
        private readonly DbContextApp _db;
        public Dictionary<string, string> Tracks;
        
        public DownloadProject(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        public IActionResult OnGet(int id)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("GetStarted");
            
            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;

            var project = _db.Songs.Find(id);
            if (project.OwnerId != user.Id && !project.IsPublicInApp)
            {
                return RedirectToPage("GetStarted");
            }

            ViewData["projectName"] = project.OriginalFileName;
            ViewData["projectArtist"] = project.Artist;
            
            Tracks = new Dictionary<string, string>();
            Tracks["Drums"] = project.DrumsUrl;
            Tracks["Bass"] = project.BassUrl;
            Tracks["Vocals"] = project.VoiceUrl;
            Tracks["Other"] = project.OtherUrl;
            return Page();
        }
    }
}
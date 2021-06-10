/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.Collections.Generic;
using System.Linq;
using isolaatti_API.isolaatti_lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using isolaatti_API.Models;

namespace isolaatti_API.Pages.admin
{
    public class AdminPortal : PageModel
    {
        //db
        private readonly DbContextApp db;
        public List<User> Last5Users;
        public AdminPortal(DbContextApp _dbContextApp)
        {
            db = _dbContextApp;
        }

        public IActionResult OnGet()
        {
            var tokenOnCookie = Request.Cookies["isolaatti_admin_session"];
            if(tokenOnCookie == null) return RedirectToPage("LogIn");
            
            var adminAccounts = new AdminAccounts(db);
            var user = adminAccounts.ValidateSessionToken(tokenOnCookie);
            if(user == null) return RedirectToPage("LogIn");
            
            // data binding here
            ViewData["username"] = user.name;
            var users = db.Users.ToList();
            var projects = db.Songs.ToList();
            var posts = db.SimpleTextPosts.ToList();
            Last5Users = users.TakeLast(5).ToList();
            ViewData["numeroDeUsuarios"] = users.Count;
            ViewData["numeroDeProyectos"] = projects.Count;
            ViewData["numeroDePublicaciones"] = posts.Count;
            return Page();
        }
    }
}
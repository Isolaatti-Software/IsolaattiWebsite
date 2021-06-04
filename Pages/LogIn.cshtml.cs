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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace isolaatti_API.Pages
{
    [IgnoreAntiforgeryToken]
    public class LogIn : PageModel
    {
        private DbContextApp _db;
        public bool WrongCredential = false;
        public bool NotVerifiedEmail = false;
        public bool NewUser = false;
        public bool JustVerifiedEmail = false;
        public bool ExistingSession = false;
        public bool ChangedPassword = false;

        public LogIn(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        
        public IActionResult OnGet(
            bool newUser=false, 
            bool badCredential=false,
            string username=null,
            bool notVerified=false,
            bool justVerified=false,
            bool changedPassword=false)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user != null) return RedirectToPage("/Index");
            
            NewUser = newUser;
            WrongCredential = badCredential;
            NotVerifiedEmail = notVerified;
            JustVerifiedEmail = justVerified;
            // ExistingSession = Request.Cookies["isolaatti_user_name"] != null &&
            //                   Request.Cookies["isolaatti_user_password"] != null &&
            //                   !WrongCredential && !NotVerifiedEmail && !JustVerifiedEmail && !NewUser;
            ChangedPassword = changedPassword;

            if (NewUser || WrongCredential || NotVerifiedEmail || JustVerifiedEmail || ChangedPassword) 
                ViewData["username_field"] = username;
            
            
            return Page();
        }
        
        public IActionResult OnPost(string email, string password)
        {
            if (email == null || password == null)
                return Page();
            
            var accountsManager = new Accounts(_db);
            try
            {
                var user = _db.Users.Single(u => u.Email.Equals(email));
                accountsManager.DefineHttpRequestObject(Request);
                var sessionToken = accountsManager.CreateNewToken(user.Id, password);
                if (sessionToken == null)
                {
                    return RedirectToPage("LogIn", new
                    {
                        badCredential = true,
                        username = email
                    });
                }
                Response.Cookies.Append("isolaatti_user_session_token", sessionToken.Token, new CookieOptions()
                {
                    Expires = new DateTimeOffset(DateTime.Today.AddMonths(1))
                });
                return RedirectToPage("Index");
            }
            catch (InvalidOperationException)
            {
                return RedirectToPage("LogIn");
            }
        }
    }
}
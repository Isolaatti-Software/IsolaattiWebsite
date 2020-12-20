/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using isolaatti_API.Models;

namespace isolaatti_API.Pages.admin
{
    public class AdminPortal : PageModel
    {
        // credentials
        private string username;
        private string password;
        
        // state
        public bool CredentialsAreValid;
        
        // data to show in tables
        public IEnumerable<Song> Songs;
        public IEnumerable<User> Users;
        
        //db
        private readonly DbContextApp db;

        public AdminAccount account;
        public AdminPortal(DbContextApp _dbContextApp)
        {
            db = _dbContextApp;
        }

        // this is called when this page is requested directly
        public IActionResult OnGet()
        {
            // look for credentials in cookies
            var username = Request.Cookies["name"];
            var password = Request.Cookies["password"];
            if (db.AdminAccounts.Any(ac => ac.name.Equals(username)))
            {
                var user = db.AdminAccounts.Single(ac => ac.name.Equals(username));
                if (user.password.Equals(password))
                {
                    return LogIn(user.name,user.password);
                }
                
            }

            // when the page is requested without parameters and no cookies have been found
            return RedirectToPage("LogIn");
        }
        public IActionResult OnPost(string username, string password)
        {
            return LogIn(username,password);
        }

        private IActionResult LogIn(string username, string password)
        {
            if (username == null || password == null) return RedirectToPage("LogIn",new {invalidsession = true});

            if (db.AdminAccounts.Any(acc => acc.name.Equals(username)))
            {
                account = db.AdminAccounts.Single(ac => ac.name.Equals(username));
                CredentialsAreValid = account.password.Equals(password);

                // account exists but password is incorrect
                if (!CredentialsAreValid) return RedirectToPage("LogIn",new {accounterror = true}) ;
                
                // name and password are both correct
                ViewData["username"] = account.name;
                this.username = account.name;
                this.password = account.password;
                
                // store data to show in tables
                Songs = db.Songs.ToArray();
                Users = db.Users.ToArray();
                
                Response.Cookies.Append("name", account.name);
                Response.Cookies.Append("password",account.password);
                return Page();

            }
            // when the account doesn't exist'
            return RedirectToPage("LogIn", new {accounterror = true});
        }

    }
}
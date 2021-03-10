/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Models;
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
                    ViewData["userId"] = user.Id;
                    
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
                        return Page();
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
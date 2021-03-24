using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Search : PageModel
    {
        private readonly DbContextApp _db;
        public List<PublicProfile> PublicProfiles = new List<PublicProfile>();
        public Search(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        public IActionResult OnGet([FromQuery] string q = "")
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
                    ViewData["password"] = user.Password;
                    ViewData["query"] = q;

                    var allAccounts = _db.Users;
                    var normalizedQuery = q.ToLower();
                    normalizedQuery = QueryNormalization.ReplaceAccents(normalizedQuery);
                    // here search for people
                    //  by email
                    foreach (var account in allAccounts.Where(account =>
                        account.Email.ToLower().Equals(normalizedQuery)))
                    {
                        PublicProfiles.Add(new PublicProfile()
                        {
                            Name = account.Name,
                            Id = account.Id
                        });
                    }

                    //  by name
                    foreach (var account in allAccounts.Where(account =>
                        account.Name.ToLower().Replace("á", "a")
                            .Replace("à", "a")
                            .Replace("é", "e")
                            .Replace("è", "e")
                            .Replace("í", "i")
                            .Replace("ì", "i")
                            .Replace("ó", "o")
                            .Replace("ò", "o")
                            .Replace("ú", "u")
                            .Replace("ù", "u")
                            .Replace("ä", "a")
                            .Replace("ë", "e")
                            .Replace("ï", "i")
                            .Replace("ö", "o")
                            .Replace("ü", "u").Contains(normalizedQuery)))
                    {
                        if (!PublicProfiles.Any(publicProfile => publicProfile.Id.Equals(account.Id)))
                        {
                            PublicProfiles.Add(new PublicProfile()
                            {
                                Name = account.Name,
                                Id = account.Id
                            });
                        }
                    }
                    // here search for user projects

                    // here search for public projects
                    return Page();
                }
            }
            catch (InvalidOperationException)
            {
                return RedirectToPage("LogIn");
            }
            catch (NullReferenceException)
            {
                return Page();
            }
            return RedirectToPage("LogIn", new
            {
                badCredential = true,
                username = email
            });
        }
    }
}
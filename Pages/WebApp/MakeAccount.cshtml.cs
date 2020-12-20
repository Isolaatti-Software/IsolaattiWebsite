/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.WebApp
{
    public class MakeAccount : PageModel
    {
        private readonly DbContextApp _db;

        public bool emailUsed = false;
        public bool nameUsed = false;

        public MakeAccount(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        public IActionResult OnGet(string user="", string email="", string error="")
        {
            if (error.Equals("emailused"))
            {
                emailUsed = true;
                ViewData["user_field"] = user;
            }

            if (error.Equals("nameused"))
            {
                nameUsed = true;
                ViewData["email_field"] = email;
            }
            
            return Page();
        }

        public IActionResult OnPost(string username, string email, string password)
        {
            if (username == null || email == null || password == null)
            {
                return Page();
            }
            var accountManager = new Accounts(_db);
            var result = accountManager.MakeAccount(username, email, password);
            switch (result)
            {
                case "0"://success
                    return RedirectToPage("LogIn", new
                    {
                        newUser=true,
                        username=email
                    });
                case "1": //email unavailable
                    return RedirectToPage("MakeAccount", new
                    {
                        user=username,
                        error="emailused"
                    });
                case "2": //name unavailable
                    return RedirectToPage("MakeAccount", new
                    {
                        email = email,
                        error="nameused"
                    });
            }
            return Page();
        }
    }
}
﻿using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class ChangePassword : PageModel
    {
        private readonly DbContextApp _db;

        public ChangePassword(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        [BindProperty] public string CurrentPassword { get; set; }

        [BindProperty] public string NewPassword { get; set; }

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
            ViewData["profilePicUrl"] = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);
            return Page();
        }

        public IActionResult OnPost()
        {
            if (CurrentPassword == null || NewPassword == null)
            {
                return RedirectToPage("MyProfile", new
                {
                    errorChangingPass = true
                });
            }

            var accountsManager = new Accounts(_db);

            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");

            // here it's know that account is correct. Data binding!
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
            ViewData["profilePicUrl"] = user.ProfileImageData == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);

            if (!accountsManager.ChangeAPassword(user.Id, CurrentPassword, NewPassword))
            {
                return RedirectToPage("MyProfile", new
                {
                    currentPasswordIsWrong = true
                });
            }

            accountsManager.RemoveAllUsersTokens(user.Id);
            return Redirect("/WebLogOut");
        }
    }
}
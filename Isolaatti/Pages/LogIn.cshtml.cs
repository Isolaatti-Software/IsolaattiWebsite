using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts;
using Isolaatti.Accounts.Service;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages
{
    public class LogIn : PageModel
    {
        private DbContextApp _db;
        private readonly IAccountsService _accounts;
        private readonly ServerRenderedAlerts _renderedAlerts;
        
        public bool NewUser { get; set; }
        public bool WrongCredential { get; set; }
        
        public LogIn(DbContextApp dbContextApp, IAccountsService accounts, ServerRenderedAlerts serverRenderedAlerts)
        {
            _db = dbContextApp;
            _accounts = accounts;
            _renderedAlerts = serverRenderedAlerts;
        }


        public async Task<IActionResult> OnGet(bool newUser = false)
        {
            var user = await _accounts.ValidateSession(SessionDto.FromJson(Request.Cookies[AccountsService.SessionCookieName]));

            NewUser = newUser;
            
            if (user != null)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPost(string? email = null, string? password = null,
            [FromQuery] string? then = null)
        {
            if (email == null || password == null)
                return Page();

            try
            {
                var user = _db.Users.Single(u => u.Email.Equals(email));

                var sessionToken = await _accounts.CreateNewSession(user.Id, password);
                if (sessionToken == null)
                {
                    WrongCredential = true;
                    return Page();
                }

                Response.Cookies.Append("isolaatti_user_session_token", sessionToken.ToString(), new CookieOptions()
                {
                    Expires = new DateTimeOffset(DateTime.Today.AddMonths(1)),
                    HttpOnly = false,
                    SameSite = SameSiteMode.Strict
                });
                if (then != null)
                {
                    return LocalRedirect(then);
                }
                
                return RedirectToPage("Index");
            }
            catch (InvalidOperationException)
            {
                return RedirectToPage("LogIn");
            }
        }
    }
}

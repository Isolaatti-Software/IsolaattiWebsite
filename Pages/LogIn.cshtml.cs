using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IAccounts _accounts;
        private readonly ServerRenderedAlerts _renderedAlerts;

        public bool WrongCredential = false;
        public bool NotVerifiedEmail = false;
        public bool NewUser = false;
        public bool JustVerifiedEmail = false;
        public bool ExistingSession = false;
        public bool ChangedPassword = false;

        public LogIn(DbContextApp dbContextApp, IAccounts accounts, ServerRenderedAlerts serverRenderedAlerts)
        {
            _db = dbContextApp;
            _accounts = accounts;
            _renderedAlerts = serverRenderedAlerts;
        }


        public async Task<IActionResult> OnGet(
            bool newUser = false,
            bool badCredential = false,
            string username = null,
            bool notVerified = false,
            bool justVerified = false,
            bool changedPassword = false,
            string then = "")
        {
            var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user != null)
            {
                return RedirectToPage("/Index");
            }

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

            ViewData["then"] = then;
            if (then != "")
            {
                _renderedAlerts.Alerts.Add("info","Debes iniciar sesión para acceder al recurso solicitado");
            }
            return Page();
        }

        public async Task<IActionResult> OnPost(string email = null, string password = null,
            [FromQuery] string then = null)
        {
            if (email == null || password == null)
                return Page();

            try
            {
                var user = _db.Users.Single(u => u.Email.Equals(email));
                if (!await _accounts.IsUserEmailVerified(user.Id))
                {
                    return RedirectToPage(new
                    {
                        notVerified = true,
                        username = user.Email
                    });
                }

                var sessionToken = await _accounts.CreateNewToken(user.Id, password);
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
                if (then != null)
                {
                    return LocalRedirect(then);
                }

                // var ipAddress = "Unavailable";
                // try
                // {
                //     ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].Last();
                // }
                // catch (InvalidOperationException)
                // {
                // }

                await _accounts.SendJustLoginEmail(user.Email, user.Name,
                    HttpContext.Connection.RemoteIpAddress?.ToString());
                return RedirectToPage("Index");
            }
            catch (InvalidOperationException)
            {
                return RedirectToPage("LogIn");
            }
        }
    }
}
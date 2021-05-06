using System;
using System.Globalization;
using System.Text.RegularExpressions;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Pages.admin.Controllers
{
    [Route("/adminControllers/[controller]")]
    public class ChangeEmail : Controller
    {
        private readonly DbContextApp _db;

        public ChangeEmail(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        [HttpPost]
        public IActionResult Index(string newEmail)
        {
            var sessionTokenFromRequest = Request.Cookies["isolaatti_admin_session"];
            if (sessionTokenFromRequest == null) return Unauthorized("Token is not present");
            
            var adminAccounts = new AdminAccounts(_db);
            var user = adminAccounts.ValidateSessionToken(sessionTokenFromRequest);
            if (user == null) return Unauthorized("Token is invalid");

            if (!IsValidEmail(newEmail))
                return RedirectToPage("/admin/AccountSettings", new
                {
                    status = AdminAccounts.StatusEmailIsNotValid
                });

            user.email = newEmail;
            _db.AdminAccounts.Update(user);
            _db.SaveChanges();
            
            return RedirectToPage("/admin/AccountSettings", new
            {
                status = AdminAccounts.StatusEmailChangedSuccessfully
            });
        }
        
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                    RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
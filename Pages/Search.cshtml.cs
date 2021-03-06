using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.isolaatti_lib;
using Isolaatti.Models;
using Isolaatti.Services;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages
{
    public class Search : PageModel
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public List<PublicProfile> PublicProfiles = new List<PublicProfile>();
        public string sessionToken;

        public Search(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        public async Task<IActionResult> OnGet([FromQuery] string q = "")
        {
            q ??= "";
            var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");
            // here it's know that account is correct. Data binding!
            sessionToken = Request.Cookies["isolaatti_user_session_token"];
            ViewData["name"] = user.Name;
            ViewData["email"] = user.Email;
            ViewData["userId"] = user.Id;
            ViewData["password"] = user.Password;
            ViewData["query"] = q;
            ViewData["profilePicUrl"] = user.ProfileImageId == null
                ? null
                : UrlGenerators.GenerateProfilePictureUrl(user.Id, sessionToken);

            var allAccounts = _db.Users;
            if (q == null || string.IsNullOrWhiteSpace(q))
            {
                return Page();
            }

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
                    Id = account.Id,
                    NumberOfFollowers = account.NumberOfFollowers,
                    NumberOfFollowing = account.NumberOfFollowing,
                    Description = account.DescriptionText
                });
            }

            //  by name
            foreach (var account in allAccounts.Where(account =>
                         account.Name.ToLower().Replace("??", "a")
                             .Replace("??", "a")
                             .Replace("??", "e")
                             .Replace("??", "e")
                             .Replace("??", "i")
                             .Replace("??", "i")
                             .Replace("??", "o")
                             .Replace("??", "o")
                             .Replace("??", "u")
                             .Replace("??", "u")
                             .Replace("??", "a")
                             .Replace("??", "e")
                             .Replace("??", "i")
                             .Replace("??", "o")
                             .Replace("??", "u").Contains(normalizedQuery)))
            {
                if (!PublicProfiles.Any(publicProfile => publicProfile.Id.Equals(account.Id)))
                {
                    PublicProfiles.Add(new PublicProfile()
                    {
                        Name = account.Name,
                        Id = account.Id,
                        NumberOfFollowers = account.NumberOfFollowers,
                        NumberOfFollowing = account.NumberOfFollowing,
                        Description = account.DescriptionText
                    });
                }
            }
            // here search for user projects

            // here search for public projects
            return Page();
        }
    }
}
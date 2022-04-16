using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Search : PageModel
    {
        private readonly DbContextApp _db;
        public List<PublicProfile> PublicProfiles = new List<PublicProfile>();
        public string sessionToken;

        public Search(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        public async Task<IActionResult> OnGet([FromQuery] string q = "")
        {
            var accountsManager = new Accounts(_db);
            var user = await accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
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
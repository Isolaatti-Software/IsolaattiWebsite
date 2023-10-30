using System.Threading.Tasks;
using Isolaatti.Accounts;
using Isolaatti.Accounts.Service;
using Isolaatti.Models;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Pages
{
    [IsolaattiAuth]
    public class Profile : IsolaattiPageModel
    {
        private readonly DbContextApp _db;
        private readonly IAccountsService _accounts;
        
        public Profile(DbContextApp dbContextApp, IAccountsService accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        public async Task<IActionResult> OnGet(string id, int numericId, [FromQuery] bool noRedirect = false)
        {
            ViewData["no-redirect"] = noRedirect;
            // get profile with id
            var profile = await _db.Users.FirstOrDefaultAsync(u => u.UniqueUsername == id || u.Id == numericId);
            if (profile == null) return NotFound();
            if (profile.Id == User.Id && !noRedirect)
            {
                return RedirectToPage("MyProfile");
            }
            ViewData["profile_id"] = profile.Id;
            ViewData["profile_name"] = profile.Name;
            
            return Page();
        }
    }
}
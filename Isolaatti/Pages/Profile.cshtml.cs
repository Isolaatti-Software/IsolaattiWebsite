using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages
{
    [IsolaattiAuth]
    public class Profile : IsolaattiPageModel
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;
        
        public Profile(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        public async Task<IActionResult> OnGet(int id, [FromQuery] bool noRedirect = false)
        {
            ViewData["no-redirect"] = noRedirect;
            // get profile with id
            var profile = await _db.Users.FindAsync(id);
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
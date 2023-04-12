using System.Threading.Tasks;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages
{
    [IsolaattiAuth]
    public class ChangePassword : IsolaattiPageModel
    {
        private readonly IAccounts _accounts;

        public ChangePassword(IAccounts accounts)
        {
            _accounts = accounts;
        }

        [BindProperty] public string CurrentPassword { get; set; }

        [BindProperty] public string NewPassword { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (CurrentPassword == null || NewPassword == null)
            {
                return RedirectToPage("MyProfile", new
                {
                    errorChangingPass = true
                });
            }
            
            if (!await _accounts.ChangeAPassword(User.Id, CurrentPassword, NewPassword))
            {
                return RedirectToPage("MyProfile", new
                {
                    currentPasswordIsWrong = true
                });
            }

            return Redirect("/LogOutPage");
        }
    }
}
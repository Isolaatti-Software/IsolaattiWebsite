using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages
{
    public class MakeAccount : PageModel
    {
        private readonly DbContextApp _db;

        public bool emailUsed = false;
        public bool nameUsed = false;
        public bool LimitOfAccountsReached = false;
        public bool AccountNotMade = false;
        private readonly IAccounts _accounts;

        public MakeAccount(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        public async Task<IActionResult> OnGet(string user = "", string email = "", string error = "",
            string referer = "", string then = "")
        {
            ViewData["alerts"] = new Dictionary<string, string>();
            if (error.Equals("emailused"))
            {
                ((Dictionary<string, string>)ViewData["alerts"])["error"] =
                    "La dirección de correo introducida ya ha sido utilizada. Intenta con otra.";
            }


            if (referer.Equals("demixer"))
            {
                ((Dictionary<string, string>)ViewData["alerts"])["info"] =
                    "Crearemos tu cuenta y te regresaremos a Demixer";
            }

            ViewData["then"] = then;

            return Page();
        }

        public async Task<IActionResult> OnPost(string username, string email, string password,
            [FromQuery] string then = "")
        {
            // if (_db.Users.Count() >= 50)
            // {
            //     return RedirectToPage("MakeAccount", new { limitOfAccounts = true });
            // }

            if (username == null || email == null || password == null)
            {
                return Page();
            }

            var result = await _accounts.MakeAccountAsync(username, email, password);
            switch (result)
            {
                case AccountMakingResult.Ok:
                    if (!then.Equals(""))
                    {
                        return Redirect(then);
                    }

                    return RedirectToPage("LogIn", new
                    {
                        newUser = true,
                        username = email
                    });
                case AccountMakingResult.EmailNotAvailable:
                    return RedirectToPage("MakeAccount", new
                    {
                        user = username,
                        error = "emailused"
                    });
                default:
                    return RedirectToPage("MakeAccount", new
                    {
                        email = email,
                        error = "couldnotsendemail"
                    });
            }
        }
    }
}
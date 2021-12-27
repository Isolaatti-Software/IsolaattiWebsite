/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System.Linq;
using System.Threading.Tasks;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SendGrid;

namespace isolaatti_API.Pages
{
    public class MakeAccount : PageModel
    {
        private readonly DbContextApp _db;

        public bool emailUsed = false;
        public bool nameUsed = false;
        public bool LimitOfAccountsReached = false;
        public bool AccountNotMade = false;
        private readonly ISendGridClient _sendGridClient;

        public MakeAccount(DbContextApp dbContextApp, ISendGridClient sendGrid)
        {
            _db = dbContextApp;
            _sendGridClient = sendGrid;
        }

        public IActionResult OnGet(string user = "", string email = "", string error = "", bool limitOfAccounts = false)
        {
            if (error.Equals("emailused"))
            {
                emailUsed = true;
                ViewData["user_field"] = user;
            }

            if (error.Equals("nameused"))
            {
                nameUsed = true;
                ViewData["email_field"] = email;
            }

            if (error.Equals("couldnotsendemail"))
            {
                ViewData["error_msg"] = "We could not verify your email address, maybe it doesn't exist";
            }

            AccountNotMade = limitOfAccounts;
            LimitOfAccountsReached = _db.Users.Count() >= 50;

            return Page();
        }

        public async Task<IActionResult> OnPost(string username, string email, string password)
        {
            if (_db.Users.Count() >= 50)
            {
                return RedirectToPage("MakeAccount", new { limitOfAccounts = true });
            }

            if (username == null || email == null || password == null)
            {
                return Page();
            }

            var accountManager = new Accounts(_db);
            accountManager.DefineHttpRequestObject(Request);
            var result = accountManager.MakeAccount(username, email, password);
            switch (result)
            {
                case "0": //success
                    await Accounts.SendWelcomeEmail(_sendGridClient, email, username);
                    return RedirectToPage("LogIn", new
                    {
                        newUser = true,
                        username = email
                    });
                case "1": //email unavailable
                    return RedirectToPage("MakeAccount", new
                    {
                        user = username,
                        error = "emailused"
                    });
                case "2": //name unavailable
                    return RedirectToPage("MakeAccount", new
                    {
                        email = email,
                        error = "nameused"
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
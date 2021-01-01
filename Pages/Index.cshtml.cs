/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages
{
    public class Index : PageModel
    {
        private readonly DbContextApp db;

        public Index(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        public IActionResult OnGet()
        {
            if (Request.Cookies["isolaatti_user_name"] != null && Request.Cookies["isolaatti_user_password"] != null)
            {
                return RedirectToPage("/WebApp/Index");
            }
            return Page();
        }
    }
}
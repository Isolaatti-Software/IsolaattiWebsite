/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    public class WebLogOut : Controller
    {
        // GET
        public IActionResult Index()
        {
            Response.Cookies.Delete("isolaatti_user_name");
            Response.Cookies.Delete("isolaatti_user_password");
            return RedirectToPage("/Index");
        }
    }
}
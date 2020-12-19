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
            return RedirectToPage("/WebApp/Index");
        }
    }
}
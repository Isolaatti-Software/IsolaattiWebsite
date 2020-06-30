using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    public class DeleteElementFromQueue : Controller
    {
        [HttpPost]
        public void Index(int elementId)
        {
            
        }
    }
}
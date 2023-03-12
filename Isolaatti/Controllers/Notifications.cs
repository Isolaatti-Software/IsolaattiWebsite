using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class Notifications : IsolaattiController
    {
        [IsolaattiAuth]
        [HttpPost]
        [Route("GetAllNotifications")]
        public IActionResult GetAll([FromForm] string sessionToken)
        {
            return Ok();
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("DeleteNotification")]
        public IActionResult DeleteANotification([FromForm] string sessionToken, [FromForm] long id)
        {
            return Ok();
        }

        [IsolaattiAuth]
        [Route("Delete/All")]
        [HttpPost]
        public IActionResult DeleteAll([FromForm] string sessionToken)
        {
            return Ok();
        }

        [IsolaattiAuth]
        [Route("MarkAsRead")]
        [HttpPost]
        public IActionResult MarkAsRead([FromForm] string sessionToken)
        {
            return Ok();
        }
    }
}
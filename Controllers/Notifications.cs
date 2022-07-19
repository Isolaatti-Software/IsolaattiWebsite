using System.Linq;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class Notifications : ControllerBase
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public Notifications(DbContextApp dbContextApp, IAccounts accounts)
        {
            _db = dbContextApp;
            _accounts = accounts;
        }

        [HttpPost]
        [Route("GetAllNotifications")]
        public IActionResult GetAll([FromForm] string sessionToken)
        {
            var user = _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            


            return Ok();
        }

        [HttpPost]
        [Route("DeleteNotification")]
        public IActionResult DeleteANotification([FromForm] string sessionToken, [FromForm] long id)
        {
            var user = _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");



            return Ok();
        }

        [Route("Delete/All")]
        [HttpPost]
        public IActionResult DeleteAll([FromForm] string sessionToken)
        {
            var user = _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            
            return Ok();
        }

        [Route("MarkAsRead")]
        [HttpPost]
        public IActionResult MarkAsRead([FromForm] string sessionToken)
        {
            var user = _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            return Ok();
        }
    }
}
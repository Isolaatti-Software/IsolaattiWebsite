using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Fetch : ControllerBase
    {
        private readonly DbContextApp _db;

        public Fetch(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        [HttpPost]
        [Route("UserData")]
        public IActionResult Index([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            return Ok(new UserData()
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Description = user.DescriptionText,
                DescriptionAudioUrl = user.DescriptionAudioUrl
            });
        }

        [HttpGet]
        [Route("GetUserProfileImage")]
        public IActionResult GetUserProfileImage(int userId)
        {
            var otherUser = _db.Users.Find(userId);
            if (otherUser == null) return NotFound("User not found");
            if (otherUser.ProfileImageData == null) return Redirect("/res/imgs/user.png");
            return new FileContentResult(otherUser.ProfileImageData, "image/png");
        }

        [HttpGet]
        [Route("rt_con")]
        public IActionResult GetAllRealTimeConnections()
        {
            return Ok(Hubs.NotificationsHub.Sessions);
        }
    }
}
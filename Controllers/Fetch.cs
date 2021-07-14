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
                ProfilePhotoUrl = ""
            });
        }

        [HttpGet]
        [Route("GetUserProfileImage")]
        public IActionResult GetUserProfileImage(int userId, string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            if (user.Id == userId)
            {
                if (user.ProfileImageData != null)
                {
                    return new FileContentResult(user.ProfileImageData,"image/png");
                }
                return NotFound("Image not found");
            }
            
            var otherUser = _db.Users.Find(userId);
            if (otherUser == null) return NotFound("User not found");
            if(otherUser.ProfileImageData == null) return NotFound("Image not found");
            return new FileContentResult(otherUser.ProfileImageData,"image/png");
        }
    }
}
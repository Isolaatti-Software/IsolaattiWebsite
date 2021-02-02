using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class EditProfile : Controller
    {
        private readonly DbContextApp Db;

        public EditProfile(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        [HttpPost]
        public IActionResult Index([FromForm]int userId, [FromForm]string password, 
            [FromForm]string newEmail, [FromForm]string newUsername)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User was not found, can't edit profile");
            if (!user.Password.Equals(password)) return Unauthorized("Password is not correct, can't edit profile");
            
            // find if there is someone else with the same username or email
            var foundRepeatedName = Db.Users
                .Any(account => account.Name.Equals(newUsername) && !account.Id.Equals(userId));
            var foundRepeatedEmail = Db.Users
                .Any(account => account.Email.Equals(newEmail) && !account.Id.Equals(userId));

            if (foundRepeatedEmail && foundRepeatedName) return Unauthorized("isolaatti_status:1");
            if (foundRepeatedEmail) return Unauthorized("isolaatti_status:2");
            if (foundRepeatedName) return Unauthorized("isolaatti_status:3");

            // no repeated profile info was found, then edit profile
            user.Name = newUsername;
            user.Email = newEmail;
            Db.Users.Update(user);
            Db.SaveChanges();
            return Ok("profile updated");
        }
    }
}
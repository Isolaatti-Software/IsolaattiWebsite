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

        [HttpPost]
        [Route("FromWeb")]
        public IActionResult FromWeb([FromForm]int id, [FromForm] string password, [FromForm] string newUsername, [FromForm] string newEmail)
        {
            var user = Db.Users.Find(id);
            if (user == null) return NotFound("User was not found");
            if (!user.Password.Equals(password)) return Unauthorized();
            
            // verify if name or email is used by someone else
            var nameRepeated = Db.Users.Any(_ => _.Name.Equals(newUsername) && !_.Id.Equals(id));
            var emailRepeated = Db.Users.Any(_ => _.Email.Equals(newEmail) && !_.Id.Equals(id));

            if (!nameRepeated && !emailRepeated)
            {
                user.Name = newUsername;
                user.Email = newEmail;
                Db.Users.Update(user);
                Db.SaveChanges();
                Response.Cookies.Append("isolaatti_user_name",newEmail);
                return RedirectToPage("/WebApp/Profile", new {profileUpdate = true});
            }

            if (nameRepeated && emailRepeated)
            {
                return RedirectToPage("/WebApp/Profile", new {nameAndEmailUsed = true});
            }

            // as the name is used by someone else, just change the email
            if (nameRepeated)
            {
                user.Email = newEmail;
                Db.Users.Update(user);
                Db.SaveChanges();
                return RedirectToPage("/WebApp/Profile", new {nameNotAvailable = true, statusData = newUsername});
            }

            user.Name = newUsername;
            Db.Users.Update(user);
            Db.SaveChanges();
            return RedirectToPage("/WebApp/Profile", new {emailNotAvailable = true, statusData = newEmail});
        }
    }
}
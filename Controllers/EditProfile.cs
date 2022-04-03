using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using isolaatti_API.Classes;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> Index([FromHeader(Name = "sessionToken")] string sessionToken,
            [FromForm] string newEmail,
            [FromForm] string newUsername)
        {
            var accountsManager = new Accounts(Db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            // find if there is someone else with the same username or email
            var foundRepeatedName = Db.Users
                .Any(account => account.Name.Equals(newUsername) && !account.Id.Equals(user.Id));
            var foundRepeatedEmail = Db.Users
                .Any(account => account.Email.Equals(newEmail) && !account.Id.Equals(user.Id));

            if (foundRepeatedEmail && foundRepeatedName) return Unauthorized("isolaatti_status:1");
            if (foundRepeatedEmail) return Unauthorized("isolaatti_status:2");
            if (foundRepeatedName) return Unauthorized("isolaatti_status:3");

            // no repeated profile info was found, then edit profile
            user.Name = newUsername;
            user.Email = newEmail;
            Db.Users.Update(user);
            await Db.SaveChangesAsync();
            return Ok("profile updated");
        }

        [HttpPost]
        [Route("FromWeb")]
        public async Task<IActionResult> FromWeb([FromForm] string newUsername, [FromForm] string newEmail,
            [FromForm] string newDescription)
        {
            var accountsManager = new Accounts(Db);
            var sessionToken = Request.Cookies["isolaatti_user_session_token"];
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            user.DescriptionText = newDescription;

            // verify if name or email is used by someone else
            var nameRepeated = Db.Users.Any(_ => _.Name.Equals(newUsername) && !_.Id.Equals(user.Id));
            var emailRepeated = Db.Users.Any(_ => _.Email.Equals(newEmail) && !_.Id.Equals(user.Id));

            if (!nameRepeated && !emailRepeated)
            {
                user.Name = newUsername;
                user.Email = newEmail;
                Db.Users.Update(user);
                await Db.SaveChangesAsync();
                Response.Cookies.Append("isolaatti_user_name", newEmail);
                return RedirectToPage("/MyProfile", new { profileUpdate = true });
            }

            if (nameRepeated && emailRepeated)
            {
                return RedirectToPage("/MyProfile", new { nameAndEmailUsed = true });
            }

            // as the name is used by someone else, just change the email
            if (nameRepeated)
            {
                user.Email = newEmail;
                Db.Users.Update(user);
                await Db.SaveChangesAsync();
                return RedirectToPage("/MyProfile", new { nameNotAvailable = true, statusData = newUsername });
            }

            user.Name = newUsername;
            Db.Users.Update(user);
            await Db.SaveChangesAsync();
            return RedirectToPage("/MyProfile", new { emailNotAvailable = true, statusData = newEmail });
        }

        [HttpPost]
        [Route("UpdatePhoto")]
        public async Task<IActionResult> UpdatePhoto([FromHeader(Name = "sessionToken")] string sessionToken,
            [FromForm] IFormFile file)
        {
            var accountsManager = new Accounts(Db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var array = stream.ToArray();

            user.ProfileImageData = array;

            Db.Users.Update(user);
            await Db.SaveChangesAsync();

            return Ok("Imagen cargada");
        }

        [HttpPost]
        [Route("UpdateAudioDescription")]
        public async Task<IActionResult> UpdateAudioDescription([FromHeader(Name = "sessionToken")] string sessionToken,
            SimpleStringData payload)
        {
            var accountsManager = new Accounts(Db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            user.DescriptionAudioUrl = payload.Data;
            Db.Users.Update(user);
            await Db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("SetProfileColor")]
        public async Task<IActionResult> SetProfileColor([FromHeader(Name = "sessionToken")] string sessionToken,
            SimpleStringData color)
        {
            var htmlColor = color.Data;
            var accountsManager = new Accounts(Db);
            var user = await accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            if (htmlColor == null) return BadRequest(new { error = "error/color-null" });

            try
            {
                ColorTranslator.FromHtml(htmlColor);
            }
            catch (Exception)
            {
                return BadRequest(new { error = "error/color-invalid" });
            }

            UserPreferences userPreferences;
            try
            {
                userPreferences = JsonSerializer.Deserialize<UserPreferences>(user.UserPreferencesJson);
            }
            catch (JsonException)
            {
                userPreferences = new UserPreferences();
            }

            userPreferences.ProfileHtmlColor = htmlColor;

            user.UserPreferencesJson = JsonSerializer.Serialize(userPreferences);

            Db.Users.Update(user);
            await Db.SaveChangesAsync();

            return Ok();
        }
    }
}
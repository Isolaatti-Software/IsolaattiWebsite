using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class EditProfile : Controller
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;
        private readonly AudiosRepository _audios;
        private readonly ImagesService _images;

        public EditProfile(DbContextApp dbContextApp, IAccounts accounts, AudiosRepository audios, ImagesService images)
        {
            _db = dbContextApp;
            _accounts = accounts;
            _audios = audios;
            _images = images;
        }

        [HttpPost]
        [Route("UpdateProfile")]
        public async Task<IActionResult> EditProfileInfo([FromHeader(Name = "sessionToken")] string sessionToken,
            EditProfileDataModel payload)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            if (payload.NewUsername.Trim().Length is < 1 or > 20)
                return BadRequest(new
                {
                    error = "Name must be between 1 and 20 characters. String is trimmed."
                });

            
            if(payload.NewUsername != null)
                user.Name = payload.NewUsername.Trim();
            if(payload.NewDescription != null)
                user.DescriptionText = payload.NewDescription.Trim();

            _db.Users.Update(user);

            await _db.SaveChangesAsync();

            return Ok(new EditProfileDataModel
            {
                NewUsername = user.Name,
                NewDescription = user.DescriptionText
            });
        }

        [HttpPost]
        [Route("RequestEmailChange")]
        public async Task<IActionResult> RequestEmailChange()
        {
            return Ok();
        }

        // [HttpPost]
        // [Route("UpdatePhoto")]
        // public async Task<IActionResult> UpdatePhoto([FromHeader(Name = "sessionToken")] string sessionToken,
        //     [FromForm] IFormFile file)
        // {
        //     var user = await _accounts.ValidateToken(sessionToken);
        //     if (user == null)
        //     {
        //         return Unauthorized("Token is not valid");
        //     }
        //
        //     var stream = new MemoryStream();
        //     await file.CopyToAsync(stream);
        //
        //     var array = stream.ToArray();
        //
        //     // add the image
        //     var profileImage = new ProfileImage
        //     {
        //         ImageData = Convert.ToBase64String(array),
        //         UserId = user.Id
        //     };
        //     _db.ProfileImages.Add(profileImage);
        //     await _db.SaveChangesAsync();
        //
        //     user.ProfileImageId = profileImage.Id;
        //     _db.Users.Update(user);
        //     await _db.SaveChangesAsync();
        //
        //     return Ok(profileImage.Id);
        // }

        [HttpPost]
        [Route("UpdateAudioDescription")]
        public async Task<IActionResult> UpdateAudioDescription([FromHeader(Name = "sessionToken")] string sessionToken,
            SimpleStringData payload)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            if (await _audios.GetAudio(payload.Data) == null)
            {
                return BadRequest("Audio specified by id does not exist");
            }

            user.DescriptionAudioId = payload.Data;

            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("SetProfilePhoto")]
        public async Task<IActionResult> UpdateProfileImageWithImageId([FromHeader(Name = "sessionToken")] string sessionToken, [FromQuery] string imageId)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null)
            {
                return Unauthorized("Token is not valid");
            }

            var selectedImage = await _images.GetImage(imageId);
            if (selectedImage == null)
            {
                return NotFound(imageId);
            }

            if (selectedImage.UserId != user.Id)
            {
                return Unauthorized();
            }
            
            user.ProfileImageId = selectedImage.Id;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            
            return Ok();
        }

        [HttpPost]
        [Route("RemoveAudioFromProfile")]
        public async Task<IActionResult> RemoveAudioFromDescription([FromHeader(Name = "sessionToken")] string sessionToken)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null)
            {
                return Unauthorized("Token is not valid");
            }

            user.DescriptionAudioId = null;

            _db.Update(user);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
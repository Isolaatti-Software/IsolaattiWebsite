using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class EditProfile : IsolaattiController
    {
        private readonly DbContextApp _db;
        private readonly AudiosRepository _audios;
        private readonly ImagesService _images;

        public EditProfile(DbContextApp dbContextApp, AudiosRepository audios, ImagesService images)
        {
            _db = dbContextApp;
            _audios = audios;
            _images = images;
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("UpdateProfile")]
        public async Task<IActionResult> EditProfileInfo(EditProfileDataModel payload)
        {
            if (payload.NewUsername.Trim().Length is < 1 or > 20)
                return BadRequest(new
                {
                    error = "Name must be between 1 and 20 characters. String is trimmed."
                });

            
            if(payload.NewUsername != null)
                User.Name = payload.NewUsername.Trim();
            if(payload.NewDescription != null)
                User.DescriptionText = payload.NewDescription.Trim();

            _db.Users.Update(User);

            await _db.SaveChangesAsync();

            return Ok(new EditProfileDataModel
            {
                NewUsername = User.Name,
                NewDescription = User.DescriptionText
            });
        }
        
        [IsolaattiAuth]
        [HttpPost]
        [Route("UpdateAudioDescription")]
        public async Task<IActionResult> UpdateAudioDescription(SimpleStringData payload)
        {
            if (await _audios.GetAudio(payload.Data) == null)
            {
                return BadRequest("Audio specified by id does not exist");
            }

            User.DescriptionAudioId = payload.Data;

            _db.Users.Update(User);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("SetProfilePhoto")]
        public async Task<IActionResult> UpdateProfileImageWithImageId([FromQuery] string imageId)
        {
            var selectedImage = await _images.GetImage(imageId);
            if (selectedImage == null)
            {
                return NotFound(imageId);
            }

            if (selectedImage.UserId != User.Id)
            {
                return Unauthorized();
            }
            
            User.ProfileImageId = selectedImage.Id;
            _db.Users.Update(User);
            await _db.SaveChangesAsync();
            
            return Ok();
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("RemoveAudioFromProfile")]
        public async Task<IActionResult> RemoveAudioFromDescription()
        {
            User.DescriptionAudioId = null;
            _db.Update(User);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
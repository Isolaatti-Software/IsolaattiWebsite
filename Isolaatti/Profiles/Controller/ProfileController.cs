using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Audios.Dto;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Images.Service;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Profiles.Controller
{
    [ApiController]
    public class ProfileController : IsolaattiController
    {
        private readonly DbContextApp _db;
        private readonly ImagesService _images;

        public ProfileController(DbContextApp db, ImagesService images)
        {
            _db = db;
            _images = images;
        }


        [IsolaattiAuth]
        [HttpGet]
        [Route("/api/Fetch/UserProfile/{userId:int}")]
        public async Task<IActionResult> GetProfile(int userId)
        {

            var account = _db.Users.Include(p => p.DescriptionAudio).FirstOrDefault(u => u.Id == userId);
            if (account == null) return NotFound();

            account.NumberOfFollowers = await _db.FollowerRelations.CountAsync(fr => fr.TargetUserId == account.Id);
            account.NumberOfFollowing = await _db.FollowerRelations.CountAsync(fr => fr.UserId == account.Id);
            account.NumberOfPosts = await _db.SimpleTextPosts.CountAsync(p => p.UserId == account.Id);
            account.NumberOfLikes = await _db.Likes.CountAsync(l => l.TargetUserId == account.Id);
            account.IsUserItself = account.Id == User.Id;
            account.ThisUserIsFollowingMe =
                await _db.FollowerRelations.AnyAsync(fr => fr.TargetUserId == User.Id && fr.UserId == account.Id);
            account.FollowingThisUser =
                await _db.FollowerRelations.AnyAsync(fr => fr.UserId == User.Id && fr.TargetUserId == account.Id);

            
            if (!account.ShowEmail && account.Id != User.Id)
            {
                account.Email = null;
            }


            return Ok(account);
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("/api/EditProfile/UpdateProfile")]
        public async Task<IActionResult> EditProfileInfo(EditProfileDto payload)
        {
            if (payload.NewUsername.Trim().Length is < 1 or > 20)
                return BadRequest(new
                {
                    error = "Name must be between 1 and 20 characters. String is trimmed."
                });


            if (payload.NewUsername != null)
                User.Name = payload.NewUsername.Trim();
            if (payload.NewDescription != null)
                User.DescriptionText = payload.NewDescription.Trim();

            _db.Users.Update(User);

            await _db.SaveChangesAsync();

            return Ok(new EditProfileDto
            {
                NewUsername = User.Name,
                NewDescription = User.DescriptionText
            });
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("/api/EditProfile/UpdateAudioDescription")]
        public async Task<IActionResult> UpdateAudioDescription(SingleIdentification<Guid> payload)
        {
            if (!_db.Audios.Any(a => a.Id.Equals(payload.Id) && a.UserId == User.Id))
            {
                return NotFound("Audio specified not found or owned");
            }
            
            User.DescriptionAudioId = payload.Id;
            
            _db.Users.Update(User);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("/api/EditProfile/SetProfilePhoto")]
        public async Task<IActionResult> UpdateProfileImageWithImageId([FromQuery] Guid imageId)
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
        [Route("/api/EditProfile/RemoveAudioFromProfile")]
        public async Task<IActionResult> RemoveAudioFromDescription()
        {
            User.DescriptionAudioId = null;
            _db.Users.Update(User);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("/api/EditProfile/RemoveProfileImage")]
        public async Task<IActionResult> RemoveProfileImage()
        {
            User.ProfileImageId = null;
            _db.Users.Update(User);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}

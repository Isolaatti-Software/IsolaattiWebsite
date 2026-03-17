using System;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Enums;
using Isolaatti.Images.Data;
using Isolaatti.Images.Service;
using Isolaatti.Squads.Repository;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace Isolaatti.Images.Controller;

[ApiController]
[Route("/api/images")]
public class ImagesController : IsolaattiController
{
    private readonly IAccountsService _accounts;
    private readonly ImagesService _images;
    private readonly SquadsRepository _squadsRepository;
    private readonly DbContextApp _db;

    public ImagesController(IAccountsService accounts, ImagesService images, SquadsRepository squadsRepository, DbContextApp db)
    {
        _accounts = accounts;
        _images = images;
        _squadsRepository = squadsRepository;
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("image/{imageId}", Name = "GetImage")]
    public async Task<IActionResult> GetImage(Guid imageId, [FromQuery] string mode)
    {
        var url = await _images.GetImageDownloadUrl(imageId, mode);
        Response.Headers.CacheControl = new StringValues(new []{"max-age=604800", $"etag={imageId}"});
        return url != null ? Redirect(url) : NotFound();
    }

    [Authorize]
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateImage([FromForm] IFormFile file, [FromQuery] bool setAsProfile, [FromForm] long? postId, [FromForm] Guid? squadId)
    {
        if (squadId != null)
        {
            var squad = await _squadsRepository.GetSquad(squadId.Value, User.Id);
            if (squad == null)
            {
                return BadRequest(new { error = "Squad doesn't exist, image not created" });
            }

            if (!await _squadsRepository.UserBelongsToSquad(squad.Id, User.Id))
            {
                return Unauthorized(new { error = "User cannot set image to squad" });
            }
        }

        // check post existence and ownership
        if (postId != null && !(await _db.SimpleTextPosts.AnyAsync(p => p.Id == postId && p.UserId == User.Id)))
        {
            return NotFound("Post not found or ownership check failed");
        }

        var image = await _images.CreateImage(file.OpenReadStream(), User.Id, squadId, postId);
        image.Username = User.Name;
        if (!setAsProfile) return Ok(image);
        if (squadId != null)
        {
            await _squadsRepository.SetSquadImage(squadId.Value, image.Id);
        }
        else
        {
            User.ProfileImageId = image.Id;
            _db.Users.Update(User);
            await _db.SaveChangesAsync();
        }
        
        return Ok(image);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("profile_image/of_user/{userId:int}")]
    public async Task<IActionResult> GetProfileImageOfUserById(int userId, string mode = "original")
    {
        var user = await _db.Users.FindAsync(userId);
        string url;
        if (user?.ProfileImageId == null)
        {
            return Redirect(user?.ProfileImageUrl ?? "/res/imgs/avatar.svg");
        }
        
        if(user.ProfileImageId == null && user.ProfileImageUrl != null)
        {
            url = user.ProfileImageUrl;
        } 
        else
        {
            url = await _images.GetImageDownloadUrl(user.ProfileImageId.Value, mode);
        }
        

        return Redirect(url);
    }

    [Authorize]
    [HttpPost]
    [Route("set_image_of_squad/{squadId:guid}")]
    public async Task<IActionResult> SetSquadImage(Guid squadId, [FromQuery] Guid imageId)
    {
        var image = await _images.GetImage(imageId);
        if (image == null)
        {
            return BadRequest("Image not found");
        }
        
        await _squadsRepository.SetSquadImage(squadId, imageId);

        return Ok();
    }

    [Authorize]
    [HttpDelete]
    [Route("{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid imageId)
    {
        var result = await _images.DeleteImage(imageId, User.Id);

        return result switch
        {
            ImageModificationResult.Success => Ok(new { message = "Success" }),
            ImageModificationResult.Error => Problem("Internal error"),
            ImageModificationResult.NotFound => NotFound(),
            ImageModificationResult.NotOwned => Unauthorized(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [Authorize]
    [HttpPost]
    [Route("delete/delete_many")]
    public async Task<IActionResult> DeleteImages([FromBody] ImagesToDeleteDto imagesToDeleteDto)
    {
        await _images.DeleteImages(imagesToDeleteDto.ImageIds, User.Id);
        
        return Ok();
    }
}

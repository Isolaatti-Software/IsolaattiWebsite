using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Controllers.Images;

[ApiController]
[Route("/api/images")]
public class ImagesController : ControllerBase
{
    private readonly IAccounts _accounts;
    private readonly ImagesService _images;
    private readonly SquadsRepository _squadsRepository;
    private readonly DbContextApp _db;

    public ImagesController(IAccounts accounts, ImagesService images, SquadsRepository squadsRepository, DbContextApp db)
    {
        _accounts = accounts;
        _images = images;
        _squadsRepository = squadsRepository;
        _db = db;
    }

    [HttpGet]
    [Route("image/{imageId}")]
    public async Task<IActionResult> GetImage(string imageId, [FromQuery] string mode)
    {
        var url = await _images.GetImageDownloadUrl(imageId, mode);
        return url != null ? Redirect(url) : NotFound();
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateImage([FromHeader(Name = "sessionToken")] string sessionToken, [FromForm] IFormFile file, [FromForm] string name, [FromQuery] bool setAsProfile, [FromForm] Guid? squadId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        if (squadId != null)
        {
            var squad = await _squadsRepository.GetSquad(squadId.Value);
            if (squad == null)
            {
                return BadRequest(new { error = "Squad doesn't exist, image not created" });
            }

            if (!await _squadsRepository.UserBelongsToSquad(user.Id, squad.Id))
            {
                return Unauthorized(new { error = "User cannot set image to squad" });
            }
        }

        var image = await _images.CreateImage(file.OpenReadStream(), user.Id, name, squadId);
        if (!setAsProfile) return Ok(image);
        if (squadId != null)
        {
            await _squadsRepository.SetSquadImage(squadId.Value, image.Id);
        }
        else
        {
            user.ProfileImageId = image.Id;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
        return Ok(image);
    }

    [HttpGet]
    [Route("profile_image/of_user/{userId:int}")]
    public async Task<IActionResult> GetProfileImageOfUserById(int userId, string mode = "original")
    {
        var imageId = await _db.Users.Where(u => u.Id == userId).Select(u => u.ProfileImageId).FirstOrDefaultAsync();
        var url = await _images.GetImageDownloadUrl(imageId, mode);
        
        return url != null ? Redirect(url) : NotFound();
    }

    [HttpGet]
    [Route("of_user/{userId:int}")]
    public async Task<IActionResult> GetImagesOfUserById(int userId, string? lastId)
    {
        return Ok(await _images.GetImagesOfUser(userId, lastId));
    }

    [HttpGet]
    [Route("of_squad/{squadId:guid}")]
    public async Task<IActionResult> GetImagesOfSquadById(Guid squadId, string lastId)
    {
        return Ok(await _squadsRepository.GetImagesOfSquad(squadId, lastId));
    }

    [HttpPost]
    [Route("set_image_of_squad/{squadId:guid}")]
    public async Task<IActionResult> SetSquadImage([FromHeader(Name = "sessionToken")] string sessionToken, Guid squadId, [FromQuery] string imageId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");
        
        var image = await _images.GetImage(imageId);
        if (image == null)
        {
            return BadRequest("Image not found");
        }
        
        await _squadsRepository.SetSquadImage(squadId, imageId);

        return Ok();
    }

}
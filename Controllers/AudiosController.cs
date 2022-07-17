using System;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.Classes.ApiEndpointsResponseDataModels;
using isolaatti_API.Models;
using isolaatti_API.Repositories;
using isolaatti_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers;

[ApiController]
[Route("/api/Audios")]
public class AudiosController : ControllerBase
{
    private readonly DbContextApp _db;
    private readonly StorageClient _storage;
    private readonly IAccounts _accounts;
    private readonly AudiosRepository _audios;

    public AudiosController(DbContextApp dbContextApp, IAccounts accounts, AudiosRepository audios)
    {
        _db = dbContextApp;
        var file = System.IO.File.Open("isolaatti-firebase-adminsdk.json", FileMode.Open, FileAccess.Read,
            FileShare.Read);
        var credential = GoogleCredential.FromStream(file);
        _storage = StorageClient.Create(credential);
        _accounts = accounts;
        _audios = audios;
    }

    [HttpGet]
    [Route("OfUser/{userId:int}")]
    public async Task<IActionResult> GetAudiosOfUser([FromHeader(Name = "sessionToken")] string sessionToken,
        int userId, [FromQuery] string lastAudioId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        return Ok(await _audios.GetAudiosOfUser(userId, lastAudioId));
    }

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> PostAudio([FromHeader(Name = "sessionToken")] string sessionToken,
        [FromForm] IFormFile audioFile, [FromForm] string name)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        var objectName = $"audios/{Guid.NewGuid()}";

        await _storage.UploadObjectAsync("isolaatti-b6641.appspot.com", objectName,
            audioFile.ContentType, audioFile.OpenReadStream());
        var createdDoc = await _audios.InsertAudio(user.Id, name, objectName);

        return Ok(createdDoc);
    }

    [HttpPost]
    [Route("{audioId}/Delete")]
    public async Task<IActionResult> DeleteAudio([FromHeader(Name = "sessionToken")] string sessionToken,
        string audioId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null)
            return Unauthorized("Token is not valid");

        var audio = await _audios.GetAudio(audioId);
        if (audio == null)
            return BadRequest(new { error = "Audio does not exist" });
        if (audio.UserId != user.Id)
            return BadRequest(new { error = "Audio cannot be deleted, it is not owned by this user" });

        await _storage.DeleteObjectAsync("isolaatti-b6641.appspot.com", audio.FirestoreObjectPath);

        await _audios.RemoveAudio(audioId);
        return Ok();
    }

    [HttpPost]
    [Route("{audioId:guid}/Rename")]
    public async Task<IActionResult> Rename([FromHeader(Name = "sessionToken")] string sessionToken, string audioId,
        SimpleStringData payload)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null)
        {
            return Unauthorized("Token is not valid");
        }

        var audio = await _audios.GetAudio(audioId);
        if (audio == null)
        {
            return NotFound(new { error = "Audio does not exist" });
        }

        if (audio.UserId != user.Id)
        {
            return Unauthorized(new { error = "This audio is not owned by the specified user" });
        }

        if (payload?.Data == null)
        {
            return BadRequest(new { error = "Name must not be null" });
        }

        if (payload.Data.Length > 30)
        {
            return BadRequest(new { error = "Name must not be more than 30 characters long" });
        }

        await _audios.RenameAudio(audioId, payload.Data);


        return Ok();
    }

    [HttpGet]
    [Route("{audioId}")]
    public async Task<IActionResult> GetAudioInformation([FromHeader(Name = "sessionToken")] string sessionToken,
        string audioId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        var audio = await _audios.GetAudio(audioId);
        if (audio == null) return NotFound();

        var feedAudio = new FeedAudio(audio);
        if (audio.UserId == user.Id)
        {
            feedAudio.UserName = user.Name;
        }
        else
        {
            feedAudio.UserName = (await _db.Users.FindAsync(feedAudio.UserId))?.Name ?? "Unknown";
        }

        return Ok(feedAudio);
    }

    [HttpGet]
    [Route("{audioId}.webm")]
    public async Task<IActionResult> PlayAudio(string audioId)
    {
        var audio = await _audios.GetAudio(audioId);
        if (audio == null) return NotFound();

        var audioObject = await _storage.GetObjectAsync("isolaatti-b6641.appspot.com", audio.FirestoreObjectPath);
        var memoryStream = new MemoryStream();
        await _storage.DownloadObjectAsync(audioObject, memoryStream);
        return new FileContentResult(memoryStream.ToArray(), audioObject.ContentType);
    }

    [HttpGet]
    [Route("Feed")]
    public async Task<IActionResult> GetNewestAudios([FromHeader(Name = "sessionToken")] string sessionToken,
        [FromQuery] string lastId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");
        var numberOfAudios = await _audios.NumberOfAudios();

        return Ok(await _audios.GetGlobalFeed(lastId));
    }
}
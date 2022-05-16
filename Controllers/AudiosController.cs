using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers;

[ApiController]
[Route("/api/Audios")]
public class AudiosController : ControllerBase
{
    private readonly DbContextApp _db;
    private readonly StorageClient _storage;

    public AudiosController(DbContextApp dbContextApp)
    {
        _db = dbContextApp;
        var file = System.IO.File.Open("isolaatti-firebase-adminsdk.json", FileMode.Open, FileAccess.Read);
        var credential = GoogleCredential.FromStream(file);
        _storage = StorageClient.Create(credential);
    }

    [HttpGet]
    [Route("OfUser/{userId:int}/{lastAudioTimestamp:datetime?}")]
    public async Task<IActionResult> GetAudiosOfUser([FromHeader(Name = "sessionToken")] string sessionToken,
        int userId, DateTime lastAudioTimestamp)
    {
        var accountsManager = new Accounts(_db);
        var user = await accountsManager.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        var audios = _db.Audios
            .Where(audio => audio.UserId == user.Id && audio.CreatedAt > lastAudioTimestamp).Take(30);
        return Ok(audios.ToList());
    }

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> PostAudio([FromHeader(Name = "sessionToken")] string sessionToken,
        [FromForm] IFormFile audioFile, [FromForm] string name)
    {
        var accountsManager = new Accounts(_db);
        var user = await accountsManager.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        var newAudio = new Audio()
        {
            CreatedAt = DateTime.Now,
            UserId = user.Id
        };

        newAudio.Name = name.Trim().Length < 1 ? "Unknown" : name;

        _db.Audios.Add(newAudio);

        await _db.SaveChangesAsync();

        var newAudioFileObject = await _storage.UploadObjectAsync("isolaatti-b6641.appspot.com",
            $"audios/{newAudio.Id}",
            audioFile.ContentType, audioFile.OpenReadStream());

        return Ok(newAudio);
    }

    [HttpPost]
    [Route("{audioId:guid}/Delete")]
    public async Task<IActionResult> DeleteAudio([FromHeader(Name = "sessionToken")] string sessionToken, Guid audioId)
    {
        var accountsManager = new Accounts(_db);
        var user = await accountsManager.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");
        return Ok();
    }

    [HttpPost]
    [Route("{audioId:guid}/Rename")]
    public async Task<IActionResult> Rename([FromHeader(Name = "sessionToken")] string sessionToken, Guid audioId,
        SimpleStringData payload)
    {
        var accountsManager = new Accounts(_db);
        var user = await accountsManager.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        var audio = await _db.Audios.FindAsync(audioId);
        if (audio == null) return NotFound();

        audio.Name = payload.Data.Trim().Length < 1 ? "Unknown" : payload.Data.Trim();
        _db.Audios.Update(audio);
        await _db.SaveChangesAsync();

        return Ok(audio);
    }

    [HttpGet]
    [Route("{audioId:guid}")]
    public async Task<IActionResult> GetAudioInformation([FromHeader(Name = "sessionToken")] string sessionToken,
        Guid audioId)
    {
        var accountsManager = new Accounts(_db);
        var user = await accountsManager.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        var audio = await _db.Audios.FindAsync(audioId);
        if (audio == null) return NotFound();


        return Ok(audio);
    }

    [HttpGet]
    [Route("{audioId:guid}/Play")]
    public async Task<IActionResult> PlayAudio(Guid audioId)
    {
        var audioObject = await _storage.GetObjectAsync("isolaatti-b6641.appspot.com", $"audios/{audioId}");
        var memoryStream = new MemoryStream();
        await _storage.DownloadObjectAsync(audioObject, memoryStream);
        return new FileContentResult(memoryStream.ToArray(), audioObject.ContentType);
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Audios")]
public class AudiosController : ControllerBase
{
    private readonly AudiosRepository _audiosRepository;
    private readonly IAccounts _accounts;
    private readonly AudiosService _audios;
    private readonly GoogleCloudStorageService _storage;

    public AudiosController(AudiosRepository audiosRepository, IAccounts accounts, AudiosService audios, GoogleCloudStorageService googleCloudStorageService)
    {
        _audiosRepository = audiosRepository;
        _accounts = accounts;
        _audios = audios;
        _storage = googleCloudStorageService;
    }

    [HttpGet]
    [Route("OfUser/{userId:int}")]
    public async Task<IActionResult> GetAudiosOfUser([FromHeader(Name = "sessionToken")] string sessionToken,
        int userId, [FromQuery] string lastAudioId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        return Ok(await _audiosRepository.GetAudiosOfUser(userId, lastAudioId));
    }

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> PostAudio([FromHeader(Name = "sessionToken")] string sessionToken,
        [FromForm] IFormFile audioFile, [FromForm] string name)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("name parameter was not provided, is empty or white spaces");
        }

        var createdDoc = await _audios.CreateAudio(audioFile.OpenReadStream(), user.Id, name, audioFile.ContentType);

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

        var result = await _audios.DeleteAudio(audioId, user.Id);
        return Ok(new
        {
            result = result.ToString()
        });
    }

    [HttpPost]
    [Route("{audioId}/Rename")]
    public async Task<IActionResult> Rename([FromHeader(Name = "sessionToken")] string sessionToken, string audioId,
        SimpleStringData payload)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null)
        {
            return Unauthorized("Token is not valid");
        }

        var result = await _audios.RenameAudio(audioId, user.Id, payload.Data);

        return Ok(new
        {
            result = result.ToString()
        });
    }

    [HttpGet]
    [Route("{audioId}")]
    public async Task<IActionResult> GetAudioInformation([FromHeader(Name = "sessionToken")] string sessionToken,
        string audioId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        var feedAudio = await _audios.GetAudio(audioId);

        if (feedAudio == null)
        {
            return NotFound();
        }

        return Ok(feedAudio);
    }

    [HttpGet]
    [Route("{audioId}.webm")]
    public async Task<IActionResult> DownloadLinkAudio(string audioId)
    {
        var audio = await _audios.GetAudio(audioId);
        if (audio == null)
        {
            return NotFound();
        }

        return Redirect(await _storage.GetDownloadUrl(audio.FirestoreObjectPath));
    }
}
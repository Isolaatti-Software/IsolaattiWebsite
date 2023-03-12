using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Audios")]
public class AudiosController : IsolaattiController
{
    private readonly AudiosRepository _audiosRepository;
    private readonly AudiosService _audios;
    private readonly GoogleCloudStorageService _storage;

    public AudiosController(AudiosRepository audiosRepository, AudiosService audios, GoogleCloudStorageService googleCloudStorageService)
    {
        _audiosRepository = audiosRepository;
        _audios = audios;
        _storage = googleCloudStorageService;
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("OfUser/{userId:int}")]
    public async Task<IActionResult> GetAudiosOfUser(int userId, [FromQuery] string lastAudioId)
    {
        return Ok(await _audiosRepository.GetAudiosOfUser(userId, lastAudioId));
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> PostAudio([FromForm] IFormFile audioFile, [FromForm] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("name parameter was not provided, is empty or white spaces");
        }

        var createdDoc = await _audios.CreateAudio(audioFile.OpenReadStream(), User.Id, name, audioFile.ContentType);

        return Ok(createdDoc);
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("{audioId}/Delete")]
    public async Task<IActionResult> DeleteAudio(string audioId)
    {
        var result = await _audios.DeleteAudio(audioId, User.Id);
        return Ok(new
        {
            result = result.ToString()
        });
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("{audioId}/Rename")]
    public async Task<IActionResult> Rename(string audioId, SimpleStringData payload)
    {
        if (payload.Data.Length < 1)
        {
            return Problem("Name cannot be empty");
        }

        var result = await _audios.RenameAudio(audioId, User.Id, payload.Data);

        return Ok(new
        {
            result = result.ToString()
        });
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("{audioId}")]
    public async Task<IActionResult> GetAudioInformation(string audioId)
    {
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
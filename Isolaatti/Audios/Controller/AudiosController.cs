using System;
using System.Threading.Tasks;
using Isolaatti.Audios.Service;
using Isolaatti.Common;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Audios.Controller;

[ApiController]
[Route("/api/Audios")]
public class AudiosController(AudiosService audios, GoogleCloudStorageService googleCloudStorageService)
    : IsolaattiController
{
    [IsolaattiAuth]
    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> PostAudio([FromForm] IFormFile audioFile, [FromForm] long postId, [FromForm] int duration)
    {
        var createdAudio = await audios.CreateAudio(audioFile.OpenReadStream(), User.Id, postId, audioFile.ContentType, duration);

        return Ok(createdAudio);
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("{audioId}/Delete")]
    public async Task<IActionResult> DeleteAudio(Guid audioId)
    {
        var result = await audios.DeleteAudio(audioId, User.Id);
        return Ok(new
        {
            result = result.ToString()
        });
    }


    [HttpGet]
    [Route("{audioId:guid}.aac")]
    public async Task<IActionResult> DownloadLinkAudio(Guid audioId)
    {
        var audio = await audios.GetAudio(audioId);
        if (audio == null)
        {
            return NotFound();
        }

        return Redirect(await googleCloudStorageService.GetDownloadUrl(audio.FirestoreObjectPath));
    }
}
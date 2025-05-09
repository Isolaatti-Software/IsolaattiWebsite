using System.Threading.Tasks;
using Isolaatti.Audios.Service;
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
        await audios.CreateAudio(audioFile.OpenReadStream(), User.Id, postId, audioFile.ContentType, duration);

        return Ok();
    }
}
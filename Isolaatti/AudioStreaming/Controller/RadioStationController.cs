using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Isolaatti.AudioStreaming.Dto;
using Isolaatti.AudioStreaming.Entity;
using Isolaatti.Config;
using Isolaatti.Models;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Isolaatti.AudioStreaming.Controller;

[ApiController]
[Route("/api/radio")]
public class RadioStationController : IsolaattiController
{
    private readonly DbContextApp _db;
    private readonly IOptions<Servers> _servers;
    private readonly ILogger<RadioStationController> _logger;

    public RadioStationController(DbContextApp db, IOptions<Servers> servers, ILogger<RadioStationController> logger)
    {
        _db = db;
        _servers = servers;
        _logger = logger;
    }

    [Route("create_station")]
    [HttpPost]
    [IsolaattiAuth]
    public async Task<IActionResult> CreateStation([FromBody] CreateStationDto createStationDto)
    {
        var station = new RadioStationEntity()
        {
            Name = createStationDto.Name,
            Description = createStationDto.Description,
            UserId = User.Id
        };

        var result = await _db.RadioStations.AddAsync(station);
        await _db.SaveChangesAsync();
        
        return Ok(new CreateStationResultDto(result.Entity));
    }

    [HttpGet]
    [Route("station/{stationId:guid}/get_stream_url")]
    [IsolaattiAuth]
    public async Task<IActionResult> GetStreamUrl(Guid stationId)
    {
        var key = RandomData.GenerateRandomKey(10);

        var station = await _db.RadioStations.FindAsync(stationId);

        if (station == null)
        {
            return NotFound();
        }

        if (station.UserId != User.Id)
        {
            return Unauthorized();
        }

        var hasher = new PasswordHasher<string>();
        var keyHash = hasher.HashPassword(station.Id.ToString(), key);

        station.KeyHash = keyHash;

        _db.RadioStations.Update(station);
        await _db.SaveChangesAsync();
        
        return Ok(_servers.Value.RtmpServer.Replace("[key]", HttpUtility.UrlEncode(key)).Replace("[streamId]", stationId.ToString()));
    }

    [HttpGet]
    [Route("station/{stationId:guid}/get_stream_play_url")]
    [IsolaattiAuth]
    public async Task<IActionResult> GetStreamPlayUrl(Guid stationId)
    {
        return Ok(new { url = _servers.Value.RtmpPlayUrl?.Replace("[stationId]", stationId.ToString()) });
    }

    [HttpPost]
    [Route("validate_key")]
    public async Task<IActionResult> ValidateKey([FromForm] string swfurl, [FromForm(Name = "name")] string stationId)
    {
        
        var uri = new Uri(swfurl);
        var query = uri.Query;
        
        var key = HttpUtility.UrlDecode(query.Trim('?'));
        
        var station = await _db.RadioStations.FindAsync(Guid.Parse(stationId));
        
        if (station == null)
        {
            return NotFound();
        }
        
        if (station.KeyHash == null)
        {
            return Unauthorized(new { message = "Generate url" });
        }
        
        var hasher = new PasswordHasher<string>();
        var result = hasher.VerifyHashedPassword(stationId, station.KeyHash, key);
        
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        return Ok();
    }
}
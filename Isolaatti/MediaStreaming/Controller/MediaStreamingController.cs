using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Isolaatti.MediaStreaming.Dto;
using Isolaatti.Config;
using Isolaatti.MediaStreaming.Entity;
using Isolaatti.Models;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Isolaatti.MediaStreaming.Controller;

[ApiController]
[Route("/api/streaming")]
public class MediaStreamingController : IsolaattiController
{
    private readonly DbContextApp _db;
    private readonly IOptions<Servers> _servers;
    private readonly ILogger<MediaStreamingController> _logger;

    public MediaStreamingController(DbContextApp db, IOptions<Servers> servers, ILogger<MediaStreamingController> logger)
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
        var station = new StreamingStationEntity()
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
    [Route("user/{userId:int}")]
    [IsolaattiAuth]
    public async Task<IActionResult> GetUserRadioStations(int userId)
    {
        return Ok(new
        {
            result = await _db.RadioStations.Where(radioStation => radioStation.UserId == userId).ToListAsync()
        });
    }

    [HttpGet]
    [Route("station/{stationId:guid}/get_stream_config")]
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

        return Ok(new
        {
            url = _servers.Value.RtmpServer.Replace("[key]", HttpUtility.UrlEncode(key)),
            name = station.Id
        });
        
    }

    [HttpGet]
    [Route("station/{stationId:guid}/get_stream_play_url")]
    [IsolaattiAuth]
    public async Task<IActionResult> GetStreamPlayUrl(Guid stationId)
    {
        return Ok(new
        {
            dash = _servers.Value.DashUrl?.Replace("[stationId]", stationId.ToString()),
            hls = _servers.Value.HlsUrl?.Replace("[stationId]", stationId.ToString())
        });
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
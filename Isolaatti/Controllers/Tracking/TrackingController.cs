using System.Linq;
using System.Threading.Tasks;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Controllers.Tracking;

[Route("/tracking")]
[ApiController]
public class TrackingController : IsolaattiController
{
    private readonly AudiosRepository _audios;
    private readonly DbContextApp _db;
    
    public TrackingController(AudiosRepository audiosRepository, DbContextApp dbContextApp)
    {
        _audios = audiosRepository;
        _db = dbContextApp;
    }
    
    [IsolaattiAuth]
    [HttpGet]
    [Route("register")]
    public async Task<IActionResult> RegisterUserInteraction([FromQuery] string audioId)
    {
        var audio = await _audios.GetAudio(audioId);
        if (audio == null)
        {
            return NotFound(new { result = "Not registered, audio not found" });
        }
        
        var record = new TrackingUserInteraction()
        {
            UserId = User.Id,
            AudioId = audioId,
            AudioDuration = audio.DurationSeconds
        };

        await _db.TrackingUserInteractions.AddAsync(record);
        await _db.SaveChangesAsync();
        
        return Ok();
    }

    [Route("getStats")]
    [HttpGet]
    public async Task<IActionResult> GetAudioStatistics()
    {
        return Ok(new StatsResultDto()
        {
            LessThanOneMinute = await _db.TrackingUserInteractions.CountAsync(tui => tui.AudioDuration <= 60),
            BetweenOneMinuteAndTwoMinutes =
                await _db.TrackingUserInteractions.CountAsync(tui => tui.AudioDuration > 60 && tui.AudioDuration <= 120),
            MoreThanTwoMinutes = await _db.TrackingUserInteractions.CountAsync(tui => tui.AudioDuration > 120)
        });
    }
}
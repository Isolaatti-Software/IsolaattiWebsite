using System;
using System.Threading.Tasks;
using Isolaatti.AudioStreaming.Dto;
using Isolaatti.AudioStreaming.Entity;
using Isolaatti.Models;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.AudioStreaming.Controller;

[ApiController]
[Route("/api/radio")]
public class RadioStationController : IsolaattiController
{
    private readonly DbContextApp _db;

    public RadioStationController(DbContextApp db)
    {
        _db = db;
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
        
        return Ok(new CreateStationResultDto(result.Entity));
    }

    [HttpGet]
    [Route("station/{stationId:guid}/get_stream_url")]
    [IsolaattiAuth]
    public async Task<IActionResult> GetStreamUrl(Guid stationId)
    {
        // se debe regresar algo de este estilo: rtmp://host:<puerto>/stream?key=<clave_generada>/<stationId>
        return Ok("");
    }

    [HttpGet]
    [Route("validate_key")]
    public async Task<IActionResult> ValidateKey([FromQuery] string key)
    {
        return Ok();
    }
}
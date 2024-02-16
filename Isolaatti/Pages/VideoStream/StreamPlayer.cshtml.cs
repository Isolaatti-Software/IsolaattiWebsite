using System;
using System.Threading.Tasks;
using Isolaatti.Config;
using Isolaatti.MediaStreaming.Entity;
using Isolaatti.Models;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Isolaatti.Pages.VideoStream;

public class StreamPlayer : IsolaattiPageModel
{
    private readonly DbContextApp _db;
    private readonly IOptions<Servers> _servers;
    
    public StreamingStationEntity? StationEntity;
    public string? DashUrl;
    public string? HlsUrl;

    public StreamPlayer(DbContextApp db, IOptions<Servers> servers)
    {
        _db = db;
        _servers = servers;
    }

    public async Task<IActionResult> OnGet([FromQuery] Guid stationId)
    {
        var station = await _db.RadioStations.FindAsync(stationId);

        if (station == null)
        {
            return NotFound();
        }

        StationEntity = station;

        DashUrl = _servers.Value.DashUrl?.Replace("[stationId]", stationId.ToString());
        HlsUrl = _servers.Value.HlsUrl?.Replace("[stationId]", stationId.ToString());

        return Page();
    }
}
using System;
using System.Threading.Tasks;
using Isolaatti.MediaStreaming.Entity;
using Isolaatti.Models;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages.MediaStreaming;

[IsolaattiAuth]
public class StreamingStation : IsolaattiPageModel
{
    private readonly DbContextApp _db;
    public StreamingStationEntity? StreamingStationEntity;

    public StreamingStation(DbContextApp db)
    {
        _db = db;
    }


    public async Task<IActionResult> OnGet([FromQuery] Guid stationId)
    {
        StreamingStationEntity = await _db.RadioStations.FindAsync(stationId);
        if (StreamingStationEntity == null || StreamingStationEntity.UserId != User.Id)
        {
            return NotFound();
        }

        ViewData["Title"] = $"Estación de transmisión: {StreamingStationEntity.Name}";
        return Page();
    }
}
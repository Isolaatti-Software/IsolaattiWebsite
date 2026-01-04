using System.Threading.Tasks;
using Isolaatti.Database.EF;
using Isolaatti.MediaStreaming.Entity;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages.MediaStreaming;

[Authorize]
public class CreateStreamingStation : IsolaattiPageModel
{
    private readonly DbContextApp _db;

    public CreateStreamingStation(DbContextApp db)
    {
        _db = db;
    }

    [BindProperty]
    public string Name { get; set; }
    
    [BindProperty]
    public string Description { get; set; }
    
    public bool ErrorName { get; set; }
    public bool ErrorDescription { get; set; }
    
    public void OnGet()
    {
        
    }

    public async Task<IActionResult> OnPost()
    {
        ErrorName = Name.IsNullOrWhiteSpace();
        ErrorDescription = Description.IsNullOrWhiteSpace();

        if (ErrorName || ErrorDescription)
        {
            return Page();
        }

        var newStation = new StreamingStationEntity()
        {
            Name = Name,
            Description = Description,
            UserId = User.Id
        };

        _db.RadioStations.Add(newStation);
        await _db.SaveChangesAsync();
        return RedirectToPage("/MediaStreaming/StreamingStation", new { stationId = newStation.Id });
    }
}
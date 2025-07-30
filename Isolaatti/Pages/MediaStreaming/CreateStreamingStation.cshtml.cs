using System.Threading.Tasks;
using Isolaatti.Helpers;
using Isolaatti.MediaStreaming.Entity;
using Isolaatti.Models;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages.MediaStreaming;

[IsolaattiAuth]
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
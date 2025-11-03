using System;
using System.Threading.Tasks;
using Isolaatti.Enums;
using Isolaatti.Squads.Repository;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages.SquadsPages;

[IsolaattiAuth]
public class SquadViewer : IsolaattiPageModel
{
    private readonly SquadsRepository _squads;
    
    public SquadViewer(SquadsRepository squadsRepository)
    {
        _squads = squadsRepository;
    }

    public Guid SquadId { get; set; }
    public bool UserBelongs { get; set; }
    
    public async Task<IActionResult> OnGet(Guid squadId)
    {
        SquadId = squadId;
        var squad = await _squads.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound();
        }

        ViewData["Title"] = squad.Name;
        UserBelongs = await _squads.UserBelongsToSquad(User.Id, squad.Id);
        if (!UserBelongs)
        {
            return NotFound();
        }

        return Page();
    }
}
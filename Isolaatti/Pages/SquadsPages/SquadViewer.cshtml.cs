using System;
using System.Threading.Tasks;
using Isolaatti.Enums;
using Isolaatti.Repositories;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages.SquadsPages;

[IsolaattiAuth]
public class SquadViewer : IsolaattiPageModel
{
    private readonly SquadsRepository _squads;
    private readonly SquadInvitationsRepository _squadInvitationsRepository;
    
    public SquadViewer(SquadsRepository squadsRepository, SquadInvitationsRepository squadInvitationsRepository)
    {
        _squads = squadsRepository;
        _squadInvitationsRepository = squadInvitationsRepository;
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
        var userWasInvited = _squadInvitationsRepository.SearchInvitation(User.Id, squad.Id) != null;
        if (!UserBelongs && squad.Privacy == SquadPrivacy.Private && !userWasInvited)
        {
            return NotFound();
        }

        return Page();
    }
}
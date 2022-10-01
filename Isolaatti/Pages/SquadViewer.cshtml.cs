using System;
using System.Threading.Tasks;
using Isolaatti.Enums;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages;

public class SquadViewer : PageModel
{
    private readonly IAccounts _accounts;
    private readonly SquadsRepository _squads;
    private readonly SquadInvitationsRepository _squadInvitationsRepository;
    
    public SquadViewer(IAccounts accounts, SquadsRepository squadsRepository, SquadInvitationsRepository squadInvitationsRepository)
    {
        _accounts = accounts;
        _squads = squadsRepository;
        _squadInvitationsRepository = squadInvitationsRepository;
    }

    public Guid SquadId { get; set; }
    public bool UserBelongs { get; set; }
    
    public async Task<IActionResult> OnGet(Guid squadId)
    {
        var user = await _accounts.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
        if (user == null) return RedirectToPage("/LogIn");

        // here it's know that account is correct. Data binding!
        ViewData["name"] = user.Name;
        ViewData["email"] = user.Email;
        ViewData["userId"] = user.Id;
        ViewData["profilePicUrl"] = user.ProfileImageId == null
            ? null
            : UrlGenerators.GenerateProfilePictureUrl(user.Id, Request.Cookies["isolaatti_user_session_token"]);

        SquadId = squadId;
        var squad = await _squads.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound();
        }

        ViewData["Title"] = squad.Name;
        UserBelongs = await _squads.UserBelongsToSquad(user.Id, squad.Id);
        var userWasInvited = _squadInvitationsRepository.SearchInvitation(user.Id, squad.Id) != null;
        if (!UserBelongs && squad.Privacy == SquadPrivacy.Private && !userWasInvited)
        {
            return NotFound();
        }

        return Page();
    }
}
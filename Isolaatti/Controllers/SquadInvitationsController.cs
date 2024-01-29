using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts;
using Isolaatti.Accounts.Service;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Squads/Invitations/{invitationId}")]
public class SquadInvitationsController : IsolaattiController
{
    public class InvitationUpdatePayload
    {
        public string? Message { get; set; }
    }
    
    private readonly IAccountsService _accounts;
    private readonly SquadInvitationsRepository _squadInvitationsRepository;
    private readonly SquadsRepository _squadsRepository;
    private readonly DbContextApp _db;
    
    public SquadInvitationsController(
        IAccountsService accounts, 
        SquadInvitationsRepository squadInvitationsRepository,
        SquadsRepository squadsRepository,
        DbContextApp dbContextApp)
    {
        _accounts = accounts;
        _squadInvitationsRepository = squadInvitationsRepository;
        _squadsRepository = squadsRepository;
        _db = dbContextApp;
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("/api/Users/Me/Squads/InvitationsForMe")]
    public async Task<IActionResult> InvitationsOfUser(string? lastId=null)
    {
        var invitations = await _squadInvitationsRepository.GetInvitationsForUser(User.Id, lastId);
        
        return Ok(invitations.Select(inv => new InvitationInfoResponse
        {
            Invitation = inv,
            SenderName = _accounts.GetUsernameFromId(inv.SenderUserId),
            RecipientName = User.Name,
            SquadName = _squadsRepository.GetSquadName(inv.SquadId) ?? string.Empty
        }));
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("/api/Users/Me/Squads/MyInvitations")]
    public async Task<IActionResult> MyInvitations(string? lastId=null)
    {
        var invitations = await _squadInvitationsRepository.GetInvitationsOfUser(User.Id, lastId);
        
        return Ok(invitations.Select(inv => new InvitationInfoResponse
        {
            Invitation = inv,
            RecipientName = _db.Users.SingleOrDefault(u => u.Id == inv.RecipientUserId)?.Name ?? string.Empty,
            SenderName = User.Name,
            SquadName = _squadsRepository.GetSquadName(inv.SquadId) ?? string.Empty
        }));
    }
    
    [IsolaattiAuth]
    [HttpDelete]
    [Route("Remove")]
    public async Task<IActionResult> RemoveInvitation(string invitationId)
    {
        var invitation = await _squadInvitationsRepository.GetInvitation(invitationId);
        if (invitation == null)
        {
            return NotFound(new { error = "Invitation does not exist" });
        }

        if (invitation.SenderUserId != User.Id)
        {
            return Unauthorized(new { error = "Invitations cannot be removed by this user" });
        }
        
        await _squadInvitationsRepository.RemoveInvitation(invitationId);
        
        return Ok();
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("Accept")]
    public async Task<IActionResult> AcceptInvitation(string invitationId, SquadInvitationAnswer payload)
    {
        var invitation = await _squadInvitationsRepository.GetInvitation(invitationId);
        if (invitation == null)
        {
            return NotFound(new { error = "Invitation does not exist." });
        }

        if (invitation.RecipientUserId != User.Id)
        {
            return Unauthorized(new { error = "This invitation cannot be accepted by this user." });
        }

        if (invitation.InvitationStatus != SquadInvitationStatus.Requested)
        {
            return Unauthorized(new { error = "This invitations has already been accepted or declined." });
        }
        
        _squadInvitationsRepository
            .UpdateInvitationStatus(invitationId, SquadInvitationStatus.Accepted, payload.Message);

        var squad = await _squadsRepository.GetSquad(invitation.SquadId);

        if (squad == null)
        {
            return Problem("Invitation was accepted, but the squad it points does not exist, maybe it was deleted.");
        }

        invitation = await _squadInvitationsRepository.GetInvitation(invitation.Id);
        return await _squadsRepository.AddUserToSquad(invitation!.SquadId, User.Id) switch
        {
            AddUserToSquadResult.Success => Ok(new
            {
                result = "Invitation accepted. User added to squad.",
                invitation = invitation
            }),
            AddUserToSquadResult.AlreadyInSquad => Ok(new
            {
                result = "Invitation had already been accepted. User is in the squad."
            }),
            AddUserToSquadResult.UserDoesNotExist => NotFound(new
            {
                result = "Validation error. User does not exist."
            }),
            AddUserToSquadResult.SquadDoesNotExist => NotFound(new
            {
                result = "Validation error. Squad does not exist."
            }),
            AddUserToSquadResult.Error => Problem("Database error occurred. Please report this."),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("Decline")]
    public async Task<IActionResult> RejectInvitation(string invitationId, SimpleStringData payload)
    {
        var invitation = await _squadInvitationsRepository.GetInvitation(invitationId);
        if (invitation == null)
        {
            return NotFound(new { error = "Invitation does not exist." });
        }

        if (invitation.RecipientUserId != User.Id)
        {
            return Unauthorized(new { error = "This invitation cannot be declined by this user." });
        }
        
        if (invitation.InvitationStatus != SquadInvitationStatus.Requested)
        {
            return Unauthorized(new { error = "This invitations has already been accepted or declined." });
        }
        
        _squadInvitationsRepository
            .UpdateInvitationStatus(invitationId, SquadInvitationStatus.Rejected, payload.Data);

        var updatedInvitation = _squadInvitationsRepository.GetInvitation(invitationId);
        
        return Ok(updatedInvitation);
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("/api/Invitations/Search")]
    public async Task<IActionResult> SearchInvitation(Guid squadId)
    {
        var invitation = _squadInvitationsRepository.SearchInvitation(User.Id, squadId);
        string? sender = null;
        if (invitation != null)
        {
            sender = (await _db.Users.FindAsync(invitation.SenderUserId))?.Name;
        }
        return Ok(new
        {
            invitation = invitation,
            senderName = sender
        });
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("/api/Squads/{squadId:guid}/Invitations")]
    public async Task<IActionResult> GetInvitationsOfSquad(Guid squadId, string? lastId = null)
    {
        return Ok(new
        {
            invitations = (await _squadInvitationsRepository.GetInvitationsOfSquad(squadId, lastId)).Select(inv => new
            {
                invitation = inv,
                senderName = _db.Users.Find(inv.SenderUserId)?.Name,
                recipientName = _db.Users.Find(inv.RecipientUserId)?.Name
            })
        });
    }

    [IsolaattiAuth]
    [HttpPut]
    [Route("Update")]
    public async Task<IActionResult> UpdateInvitation(InvitationUpdatePayload invitationUpdatePayload, string invitationId)
    {
        var invitation = await _squadInvitationsRepository.GetInvitation(invitationId);
        if (invitation == null)
        {
            return NotFound();
        }

        if (invitation.SenderUserId != User.Id)
        {
            return Unauthorized();
        }
        
        _squadInvitationsRepository.UpdateInvitationMessage(invitationId, invitationUpdatePayload.Message);

        return Ok();
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Squads/Invitations/{invitationId}")]
public class SquadInvitationsController : ControllerBase
{
    private readonly IAccounts _accounts;
    private readonly SquadInvitationsRepository _squadInvitationsRepository;
    private readonly SquadsRepository _squadsRepository;
    private readonly DbContextApp _db;
    
    public SquadInvitationsController(
        IAccounts accounts, 
        SquadInvitationsRepository squadInvitationsRepository,
        SquadsRepository squadsRepository,
        DbContextApp dbContextApp)
    {
        _accounts = accounts;
        _squadInvitationsRepository = squadInvitationsRepository;
        _squadsRepository = squadsRepository;
        _db = dbContextApp;
    }

    [HttpGet]
    [Route("/api/Users/Me/Squads/InvitationsForMe")]
    public async Task<IActionResult> InvitationsOfUser(
        [FromHeader(Name = "sessionToken")] string sessionToken,
        string lastId=null)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

        var invitations = await _squadInvitationsRepository.GetInvitationsForUser(user.Id, lastId);
        
        return Ok(invitations.Select(inv => new InvitationInfoResponse
        {
            Invitation = inv,
            SenderName = _accounts.GetUsernameFromId(inv.SenderUserId),
            RecipientName = user.Name,
            SquadName = _squadsRepository.GetSquadName(inv.SquadId)
        }));
    }

    [HttpGet]
    [Route("/api/Users/Me/Squads/MyInvitations")]
    public async Task<IActionResult> MyInvitations([FromHeader(Name = "sessionToken")] string sessionToken,
        string lastId=null)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

        var invitations = await _squadInvitationsRepository.GetInvitationsOfUser(user.Id, lastId);
        
        return Ok(invitations.Select(inv => new InvitationInfoResponse
        {
            Invitation = inv,
            RecipientName = _db.Users.SingleOrDefault(u => u.Id == inv.RecipientUserId)?.Name,
            SenderName = user.Name,
            SquadName = _squadsRepository.GetSquadName(inv.SquadId)
        }));
    }
    
    

    [HttpDelete]
    public async Task<IActionResult> RemoveInvitation([FromHeader(Name = "sessionToken")] string sessionToken,
        string invitationId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

        var invitation = _squadInvitationsRepository.GetInvitation(invitationId);
        if (invitation == null)
        {
            return NotFound(new { error = "Invitation does not exist" });
        }

        if (invitation.SenderUserId != user.Id)
        {
            return Unauthorized(new { error = "Invitations cannot be removed by this user" });
        }
        
        await _squadInvitationsRepository.RemoveInvitation(invitationId);
        
        return Ok();
    }

    [HttpPost]
    [Route("Accept")]
    public async Task<IActionResult> AcceptInvitation(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        string invitationId, 
        SquadInvitationAnswer payload)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");
        
        var invitation = _squadInvitationsRepository.GetInvitation(invitationId);
        if (invitation == null)
        {
            return NotFound(new { error = "Invitation does not exist." });
        }

        if (invitation.RecipientUserId != user.Id)
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

        invitation = _squadInvitationsRepository.GetInvitation(invitation.Id);
        return await _squadsRepository.AddUserToSquad(invitation.SquadId, user.Id) switch
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

    [HttpPost]
    [Route("Decline")]
    public async Task<IActionResult> RejectInvitation(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        string invitationId, 
        SquadInvitationAnswer payload)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");
        
        var invitation = _squadInvitationsRepository.GetInvitation(invitationId);
        if (invitation == null)
        {
            return NotFound(new { error = "Invitation does not exist." });
        }

        if (invitation.RecipientUserId != user.Id)
        {
            return Unauthorized(new { error = "This invitation cannot be declined by this user." });
        }
        
        if (invitation.InvitationStatus != SquadInvitationStatus.Requested)
        {
            return Unauthorized(new { error = "This invitations has already been accepted or declined." });
        }
        
        _squadInvitationsRepository
            .UpdateInvitationStatus(invitationId, SquadInvitationStatus.Rejected, payload.Message);
        
        return Ok();
    }

    [HttpGet]
    [Route("/api/Invitations/Search")]
    public async Task<IActionResult> SearchInvitation([FromHeader(Name = "sessionToken")] string sessionToken, Guid squadId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");
        var invitation = _squadInvitationsRepository.SearchInvitation(user.Id, squadId);
        string sender = null;
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

    [HttpGet]
    [Route("/api/Squads/{squadId:guid}/Invitations")]
    public async Task<IActionResult> GetInvitationsOfSquad([FromHeader(Name = "sessionToken")] string sessionToken, Guid squadId, string lastId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");
        
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
}
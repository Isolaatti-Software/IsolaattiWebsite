using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.DTOs;
using Isolaatti.EmailSender;
using Isolaatti.Enums;
using Isolaatti.isolaatti_lib;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Squads")]
public class SquadsController : IsolaattiController
{
    private readonly SquadsRepository _squadsRepository;
    private readonly SquadInvitationsRepository _squadInvitationsRepository;
    private readonly DbContextApp _db;
    private readonly EmailSenderMessaging _emailSenderMessaging;

    public SquadsController(
        SquadsRepository squadsRepository, 
        SquadInvitationsRepository squadInvitationsRepository,
        SquadJoinRequestsRepository squadJoinRequestsRepository,
        DbContextApp dbContextApp, 
        EmailSenderMessaging emailSenderMessaging)
    {
        _squadsRepository = squadsRepository;
        _squadInvitationsRepository = squadInvitationsRepository;
        _db = dbContextApp;
        _emailSenderMessaging = emailSenderMessaging;
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> MakeSquad(SquadCreationRequest payload)
    {
        var creationResult = await _squadsRepository.MakeSquad(User.Id, payload);
        return Ok(creationResult);
    }
    
    [IsolaattiAuth]
    [HttpGet]
    [Route("{squadId:guid}")]
    public async Task<IActionResult> GetSquad(Guid squadId)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return BadRequest("Squad not found");
        }
        return Ok(new
        {
            squad = squad,
            state = new SquadStateDto()
            {
                IsOwner = squad.UserId == User.Id
            }
        });
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("{squadId:guid}/MyState")]
    public async Task<IActionResult> GetSquadState(Guid squadId)
    {
        return Ok(await _squadsRepository.GetSquadState(squadId, User.Id));
    }
    
    [IsolaattiAuth]
    [HttpDelete]
    [Route("{squadId:guid}")]
    public async Task<IActionResult> RemoveSquad(Guid squadId)
    {
        if (!await _squadsRepository.ValidateSquadOwner(squadId, User.Id))
        {
            return Unauthorized(new { error = "This squad cannot by deleted by this user." });
        }

        await _squadInvitationsRepository.RemoveInvitationsForASquad(squadId);

        await _squadsRepository.RemoveSquad(squadId);
        
        return Ok();
    }
    
    [IsolaattiAuth]
    [HttpPost]
    [Route("{squadId:guid}/Invite")]
    public async Task<IActionResult> MakeInvitation(Guid squadId, SquadInvitationCreationRequest payload)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new { error = "Squad does not exist." });
        }

        var recipientUser = await _db.Users.FindAsync(payload.UserId);
        if (recipientUser == null)
        {
            return NotFound(new { error = "User does not exist." });
        }

        if (await _squadInvitationsRepository.SameInvitationExists(squad.Id, User.Id, recipientUser.Id))
        {
            return Ok(new { error = "Invitation had already been sent." });
        }
        await _squadInvitationsRepository
            .CreateInvitation(squadId, User.Id, payload.UserId, payload.Message);

        _emailSenderMessaging.SendEmail(
            EmailTemplates.NotificationsAddress, 
            "Isolaatti", 
            recipientUser.Email, 
            recipientUser.Name, 
            "Invitación a squad", 
            string.Format(EmailTemplates.InvitationEmail, User.UniqueUsername, squad.Name));
        
        return Ok(new { result = "Invitation sent." });
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("{squadId:guid}/InviteMany")]
    public async Task<IActionResult> MakeInvitations(Guid squadId, SquadInvitationsCreationRequest payload)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new { error = "Squad does not exist." });
        }

        // This list of ids may have ids that does not belong to any user. It is necessary to test.
        var userIds = payload.UserIds;

        var users = await (from user in _db.Users
            where userIds.Contains(user.Id)
            select new { id = user.Id, username = user.UniqueUsername, displayName = user.Name, email = user.Email }).ToListAsync();
        
        // TODO: Fix this
        // // Now I remove the users that had already received an invitation, no matter the message.
        // realUserIds = await realUserIds.ToAsyncEnumerable()
        //     .WhereAwait(async el => 
        //         await _squadInvitationsRepository.SameInvitationExists(squad.Id, user.Id, el))
        //     .ToListAsync();


        if (users.Count < 1)
        {
            return NotFound(new { error = "No invitations were sent, the passed ids are fake." });
        }

        foreach (var u in users)
        {
            _emailSenderMessaging.SendEmail(
                EmailTemplates.NotificationsAddress, 
                "Isolaatti", 
                u.email, 
                u.displayName, 
                "Invitación a squad", 
                string.Format(EmailTemplates.InvitationEmail, User.UniqueUsername, squad.Name));
        }

        await _squadInvitationsRepository.CreateInvitations(squadId, User.Id, users.Select(u => u.id), payload.Message);
        return Ok(new { result = "Invitations send. Fake ids or ids that had already been used, if any, were omitted."});
    }
    

    [IsolaattiAuth]
    [HttpGet]
    [Route("MySquads")]
    public async Task<IActionResult> GetSquadOfUserAdmins(Guid? lastId)
    {
        return Ok(await _squadsRepository.GetSquadsUserOwns(User.Id, lastId).ToListAsync());
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("SquadsBelong")]
    public async Task<IActionResult> GetSquadsOfUser(Guid? lastId)
    {
        return Ok(_squadsRepository.GetSquadUserBelongs(User.Id, lastId));
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("{squadId:guid}/Update")]
    public async Task<IActionResult> UpdateSquad(Guid squadId, SquadUpdateInfoRequest payload)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new { error = "Squad does not exist" });
        }

        if (!squad.UserId.Equals(User.Id))
        {
            return Unauthorized(new { error = "Squad cannot be updated by this user" });
        }

        return Ok(new
        {
            result = (await _squadsRepository.UpdateSquad(squadId, payload.Name, payload.Description,
                payload.ExtendedDescription)).ToString()
        });
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("{squadId:guid}/UpdatePrivacyTo/{privacy}")]
    public async Task<IActionResult> UpdatePrivacy(Guid squadId, string privacy)
    {
        SquadPrivacy squadPrivacy;
        try 
        {
            squadPrivacy = Enum.Parse<SquadPrivacy>(privacy);
        }
        catch(ArgumentException) 
        {
            return BadRequest(new 
            {
                message = "Provide value for route param privacy: Private or Public"
            });
        }

        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new { error = "Squad does not exist" });
        }

        if (!squad.UserId.Equals(User.Id))
        {
            return Unauthorized(new { error = "Squad cannot be updated by this user" });
        }

        await _squadsRepository.SetSquadPrivacy(squadId, squadPrivacy);

        return Ok();
    }
}
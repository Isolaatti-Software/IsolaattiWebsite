using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Enums;
using Isolaatti.Helpers;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Squads")]
public class SquadsController : ControllerBase
{
    private readonly IAccounts _accounts;
    private readonly SquadsRepository _squadsRepository;
    private readonly SquadInvitationsRepository _squadInvitationsRepository;
    private readonly SquadJoinRequestsRepository _squadJoinRequestsRepository;
    private readonly DbContextApp _db;

    public SquadsController(
        IAccounts accounts, 
        SquadsRepository squadsRepository, 
        SquadInvitationsRepository squadInvitationsRepository,
        SquadJoinRequestsRepository squadJoinRequestsRepository,
        DbContextApp dbContextApp)
    {
        _accounts = accounts;
        _squadsRepository = squadsRepository;
        _squadInvitationsRepository = squadInvitationsRepository;
        _squadJoinRequestsRepository = squadJoinRequestsRepository;
        _db = dbContextApp;
    }

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> MakeSquad(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        SquadCreationRequest payload)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");
        
        var creationResult = await _squadsRepository.MakeSquad(user.Id, payload);
        return Ok(creationResult);
    }
    
    [HttpGet]
    [Route("{squadId:guid}")]
    public async Task<IActionResult> GetSquad(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        Guid squadId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");
        
        
        return Ok(await _squadsRepository.GetSquad(squadId));
    }
    
    [HttpDelete]
    [Route("{squadId:guid}")]
    public async Task<IActionResult> RemoveSquad(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        Guid squadId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");
        if (!await _squadsRepository.ValidateSquadOwner(squadId, user.Id))
        {
            return Unauthorized(new { error = "This squad cannot by deleted by this user." });
        }

        await _squadInvitationsRepository.RemoveInvitationsForASquad(squadId);

        await _squadsRepository.RemoveSquad(squadId);
        
        return Ok();
    }
    
    [HttpPost]
    [Route("{squadId:guid}/Invite")]
    public async Task<IActionResult> MakeInvitation(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        Guid squadId, 
        SquadInvitationCreationRequest payload)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

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

        if (await _squadInvitationsRepository.SameInvitationExists(squad.Id, user.Id, recipientUser.Id))
        {
            return Ok(new { error = "Invitation had already been sent." });
        }
        await _squadInvitationsRepository
            .CreateInvitation(squadId, user.Id, payload.UserId, payload.Message);

        return Ok(new { result = "Invitation sent." });
    }

    [HttpPost]
    [Route("{squadId:guid}/InviteMany")]
    public async Task<IActionResult> MakeInvitations(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        Guid squadId,
        SquadInvitationsCreationRequest payload)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");
        
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new { error = "Squad does not exist." });
        }

        // This list of ids may have ids that does not belong to any user. It is necessary to test.
        var userIds = payload.UserIds;
        var realUserIds = new List<int>();
        foreach (var userId in userIds)
        {
            if (await _db.Users.AnyAsync(u => u.Id.Equals(userId)))
            {
                realUserIds.Add(userId);
            }
        }
        
        // TODO: Fix this
        // // Now I remove the users that had already received an invitation, no matter the message.
        // realUserIds = await realUserIds.ToAsyncEnumerable()
        //     .WhereAwait(async el => 
        //         await _squadInvitationsRepository.SameInvitationExists(squad.Id, user.Id, el))
        //     .ToListAsync();


        if (realUserIds.Count < 1)
        {
            return NotFound(new { error = "No invitations were sent, the passed ids are fake." });
        }

        await _squadInvitationsRepository.CreateInvitations(squadId, user.Id, realUserIds, payload.Message);
        return Ok(new { result = "Invitations send. Fake ids or ids that had already been used, if any, were omitted."});
    }

    // [HttpPost]
    // [Route("{squadId:guid}/RequestJoin")]
    // public async Task<IActionResult> RequestJoin(
    //     [FromHeader(Name = "sessionToken")] string sessionToken, 
    //     Guid squadId,
    //     SquadJoinRequestCreationRequest payload)
    // {
    //     var user = await _accounts.ValidateToken(sessionToken);
    //     if(user == null) return Unauthorized("Token is not valid");
    //
    //     var squad = await _squadsRepository.GetSquad(squadId);
    //     if (squad == null)
    //     {
    //         return NotFound(new { error = "Squad does not exist." });
    //     }
    //
    //     if (await _squadJoinRequestsRepository.SameJoinRequestExists(squadId, user.Id))
    //     {
    //         return Ok(new
    //         {
    //             error = "Cannot request joining again. Manually delete the previous request before sending a new one"
    //         });
    //     }
    //
    //     await _squadJoinRequestsRepository.CreateJoinRequest(squadId, user.Id, payload.Message);
    //
    //     return Ok(new
    //     {
    //         result = "Join request sent"
    //     });
    // }

    
    [HttpGet]
    [Route("MySquads")]
    public async Task<IActionResult> GetSquadOfUserAdmins(
        [FromHeader(Name = "sessionToken")] 
        string sessionToken, 
        Guid lastId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

        return Ok(await _squadsRepository.GetSquadsUserAdmins(user.Id, lastId).ToListAsync());
    }

    [HttpGet]
    [Route("SquadsBelong")]
    public async Task<IActionResult> GetSquadsOfUser(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        Guid lastId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

        return Ok(_squadsRepository.GetSquadUserBelongs(user.Id, lastId));
    }

    [HttpGet]
    [Route("{squadId:guid}/Members")]
    public async Task<IActionResult> GetMembersOfSquad(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        Guid squadId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound();
        }

        var admin = (from u in _db.Users
            where u.Id == squad.UserId
            select new
            {
                Id = u.Id,
                Name = u.Name,
                ImageId = u.ProfileImageId
            }).FirstOrDefault();

        return Ok(new
        {
            members = await _squadsRepository.GetMembersOfSquad(squadId),
            admin
        });
    }

    [HttpPost]
    [Route("{squadId:guid}/Update")]
    public async Task<IActionResult> UpdateSquad(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        Guid squadId, 
        SquadUpdateInfoRequest payload)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new { error = "Squad does not exist" });
        }

        if (!squad.UserId.Equals(user.Id))
        {
            return Unauthorized(new { error = "Squad cannot be updated by this user" });
        }

        return Ok(new
        {
            result = (await _squadsRepository.UpdateSquad(squadId, payload.Name, payload.Description,
                payload.ExtendedDescription)).ToString()
        });
    }

    [HttpPost]
    [Route("{squadId:guid}/UpdatePrivacyTo/{privacy}")]
    public async Task<IActionResult> UpdatePrivacy(
        [FromHeader(Name = "sessionToken")] string sessionToken,
        Guid squadId, string privacy)
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

        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new { error = "Squad does not exist" });
        }

        if (!squad.UserId.Equals(user.Id))
        {
            return Unauthorized(new { error = "Squad cannot be updated by this user" });
        }

        await _squadsRepository.SetSquadPrivacy(squadId, squadPrivacy);

        return Ok();
    }
}
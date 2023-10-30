using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts;
using Isolaatti.Accounts.Service;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Squads/JoinRequests")]
public class SquadJoinRequestsController : IsolaattiController
{
    private readonly IAccountsService _accounts;
    private readonly DbContextApp _db;
    private readonly SquadJoinRequestsRepository _joinRequestsRepository;
    private readonly SquadsRepository _squadsRepository;
    
    public SquadJoinRequestsController(IAccountsService accounts, 
        DbContextApp dbContextApp, 
        SquadJoinRequestsRepository joinRequestsRepository,
        SquadsRepository squadsRepository)
    {
        _accounts = accounts;
        _db = dbContextApp;
        _joinRequestsRepository = joinRequestsRepository;
        _squadsRepository = squadsRepository;
    }
    
    [IsolaattiAuth]
    [HttpGet]
    [Route("Search")]
    public async Task<IActionResult> Search(Guid squadId)
    {
        return Ok(new
        {
            request = await _joinRequestsRepository.SearchJoinRequest(squadId, User.Id)
        });
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("/api/Squads/{squadId:guid}/RequestJoin")]
    public async Task<IActionResult> Create(Guid squadId, CreateJoinRequest payload)
    {
        if (await _joinRequestsRepository.SameJoinRequestExists(squadId, User.Id))
        {
            return Problem("Already sent join request. User has to wait until accepted.");
        }

        await _joinRequestsRepository.CreateJoinRequest(squadId, User.Id, payload.Message);
        
        return Ok();
    }

    /// <summary>
    /// Removes the request. Only creator can do this.
    /// </summary>
    /// <param name="sessionToken">Auth token</param>
    /// <param name="requestId">Join request id</param>
    /// <returns></returns>
    [IsolaattiAuth]
    [HttpDelete]
    [Route("{requestId}/Remove")]
    public async Task<IActionResult> Remove(string requestId)
    {
        var joinRequest = _joinRequestsRepository.GetJoinRequest(requestId);
        if (joinRequest == null)
        {
            return NotFound();
        }

        if (joinRequest.SenderUserId != User.Id)
        {
            return Unauthorized(new
            {
                error = "Unauthorized. Request cannot by deleted"
            });
        }

        await _joinRequestsRepository.RemoveJoinRequest(requestId);
        return Ok(new
        {
            result = "Join request deleted successfully"
        });
    }

    /// <summary>
    /// Accept or reject the user's join request to a squad.
    /// </summary>
    /// <param name="sessionToken">Auth token</param>
    /// <param name="requestId">Request id provided by list join request endpoint</param>
    /// <param name="action">'Accept' or 'Reject' are the only valid actions</param>
    /// <param name="payload">Object containing the reason message. The string can be null but not the object.</param>
    /// <returns></returns>
    [IsolaattiAuth]
    [HttpPost]
    [Route("{requestId}/Reject")]
    public async Task<IActionResult> Reject(string requestId, SimpleStringData payload)
    {
        // Validates join request's existence
        var joinRequest = _joinRequestsRepository.GetJoinRequest(requestId);
        if (joinRequest == null)
        {
            return NotFound();
        }

        // Validates squad's existence
        var squad = await _squadsRepository.GetSquad(joinRequest.SquadId);
        if (squad == null)
        {
            return NotFound(new
            {
                error = "The squad this join request is pointing was not found"
            });
        }

        // Validates the user is admin
        if (squad.UserId != User.Id)
        {
            return Unauthorized(new
            {
                error = "You are not admin. Only admins can reject requests"
            });
        }

        
        _joinRequestsRepository.UpdateJoinRequest(requestId, SquadInvitationStatus.Rejected, payload.Data);
        joinRequest = _joinRequestsRepository.GetJoinRequest(requestId);
        
        return Ok(new
        {
            request = joinRequest,
            senderName = _accounts.GetUsernameFromId(joinRequest.SenderUserId),
            squadName = _squadsRepository.GetSquadName(joinRequest.SquadId),
            admins = squad.UserId == User.Id
        });
    }

    /// <summary>
    /// Endpoint that returns join request that this squad has received
    /// </summary>
    /// <param name="sessionToken">Auth token</param>
    /// <param name="squadId">Squad id</param>
    /// <param name="lastId">Last item served for pagination</param>
    /// <returns></returns>
    [IsolaattiAuth]
    [HttpGet]
    [Route("/api/Squads/{squadId:guid}/JoinRequests")]
    public async Task<IActionResult> ListRequestsOfSquad(Guid squadId, string lastId = null)
    { 
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new
            {
                error = "Squad not found"
            });
        }

        var userIsAdminOrMember = squad.UserId.Equals(User.Id) 
                                  || await _squadsRepository.UserBelongsToSquad(User.Id, squadId);
        if (!userIsAdminOrMember)
        {
            return Unauthorized(new
            {
                error = "Unauthorized. You cannot access join requests of this squad because you are not a member or admin"
            });
        }

        var requests = await _joinRequestsRepository.GetJoinRequestsOfSquad(squad.Id, lastId);

        
        return Ok(requests.Select(request => new
        {
            request,
            senderName = _db.Users.Where(u => u.Id == request.SenderUserId).Select(u => u.Name).FirstOrDefault(),
            squadName = _squadsRepository.GetSquadName(request.SquadId),
            admins = squad.UserId == User.Id
        }));
    }

    /// <summary>
    /// Endpoint that returns the join requests the user has made
    /// </summary>
    /// <param name="sessionToken">Auth token</param>
    /// <param name="lastId">Last join request served id for pagination.</param>
    /// <returns></returns>
    [IsolaattiAuth]
    [HttpGet]
    [Route("MyJoinRequests")]
    public async Task<IActionResult> ListRequestsMadeOfUser(string lastId = null)
    {
        var joinRequests = await _joinRequestsRepository.GetJoinRequestsOfUser(User.Id, lastId);
        
        return Ok(joinRequests.Select(request => new
        {
            request,
            senderName = _accounts.GetUsernameFromId(request.SenderUserId),
            squadName = _squadsRepository.GetSquadName(request.SquadId),
            admins = false
        }));
    }
    /// <summary>
    /// Endpoint that returns the join requests the user has received in the squads this admins
    /// </summary>
    /// <param name="sessionToken">Auth token</param>
    /// <param name="lastId">Last join request served id for pagination.</param>
    /// <returns></returns>
    [IsolaattiAuth]
    [HttpGet]
    [Route("JoinRequestsForMe")]
    public async Task<IActionResult> ListRequestsMadeForUser(string lastId = null)
    {
        var joinRequests = await _joinRequestsRepository.GetJoinRequestsForUser(User.Id, lastId);
        
        return Ok(joinRequests.Select(request => new
        {
            request,
            senderName = _accounts.GetUsernameFromId(request.SenderUserId),
            squadName = _squadsRepository.GetSquadName(request.SquadId),
            admins = true
        }));
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("{requestId}/Accept")]
    public async Task<IActionResult> AcceptJoinRequest(string requestId, SimpleStringData message)
    {
        var joinRequest = _joinRequestsRepository.GetJoinRequest(requestId);

        if (joinRequest == null)
        {
            return NotFound();
        }

        var squad = await _squadsRepository.GetSquad(joinRequest.SquadId);
        if (squad.UserId != User.Id)
        {
            return Unauthorized();
        }
        
        var result =_joinRequestsRepository.UpdateJoinRequest(requestId, SquadInvitationStatus.Accepted, message.Data);

        joinRequest = _joinRequestsRepository.GetJoinRequest(requestId);
        await _squadsRepository.AddUserToSquad(squad.Id, joinRequest.SenderUserId);
        if (result)
        {
            return Ok(new
            {
                request = joinRequest,
                senderName = _accounts.GetUsernameFromId(joinRequest.SenderUserId),
                squadName = _squadsRepository.GetSquadName(joinRequest.SquadId),
                admins = squad.UserId == User.Id
            });
        }
        return NotFound();
    }
}
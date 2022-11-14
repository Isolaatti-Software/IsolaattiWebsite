using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Squads/JoinRequests")]
public class SquadJoinRequestsController : ControllerBase
{
    private readonly IAccounts _accounts;
    private readonly DbContextApp _db;
    private readonly SquadJoinRequestsRepository _joinRequestsRepository;
    private readonly SquadsRepository _squadsRepository;
    
    public SquadJoinRequestsController(IAccounts accounts, 
        DbContextApp dbContextApp, 
        SquadJoinRequestsRepository joinRequestsRepository,
        SquadsRepository squadsRepository)
    {
        _accounts = accounts;
        _db = dbContextApp;
        _joinRequestsRepository = joinRequestsRepository;
        _squadsRepository = squadsRepository;
    }
    
    [HttpGet]
    [Route("Search")]
    public async Task<IActionResult> Search([FromHeader(Name = "sessionToken")] string sessionToken, Guid squadId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");
        
        return Ok(new
        {
            request = await _joinRequestsRepository.SearchJoinRequest(squadId, user.Id)
        });
    }

    [HttpPost]
    [Route("/api/Squads/{squadId:guid}/RequestJoin")]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "sessionToken")] string sessionToken,
        Guid squadId,
        CreateJoinRequest payload)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

        if (await _joinRequestsRepository.SameJoinRequestExists(squadId, user.Id))
        {
            return Problem("Already sent join request. User has to wait until accepted.");
        }

        await _joinRequestsRepository.CreateJoinRequest(squadId, user.Id, payload.Message);
        
        return Ok();
    }

    /// <summary>
    /// Removes the request. Only creator can do this.
    /// </summary>
    /// <param name="sessionToken">Auth token</param>
    /// <param name="requestId">Join request id</param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{requestId}/Remove")]
    public async Task<IActionResult> Remove([FromHeader(Name = "sessionToken")] string sessionToken, string requestId)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

        var joinRequest = _joinRequestsRepository.GetJoinRequest(requestId);
        if (joinRequest == null)
        {
            return NotFound();
        }

        if (joinRequest.SenderUserId != user.Id)
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
    [HttpPost]
    [Route("{requestId}/{action}")]
    public async Task<IActionResult> Reject(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        string requestId,
        string action,
        SquadInvitationAnswer payload)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");
        
        // Performs validation for action param
        if (!action.Equals("Reject") || action.Equals("Accept"))
        {
            return Problem("Action path param does not match 'Reject' or 'Accept'.");
        }

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
        if (squad.UserId != user.Id)
        {
            return Unauthorized(new
            {
                error = "You are not admin. Only admins can reject requests"
            });
        }

        // Validates as a request that was already rejected cannot be changed.
        if (joinRequest.JoinRequestStatus != SquadInvitationStatus.Requested)
        {
            return Problem("This request cannot be rejected, it was accepted or rejected already");
        }

        switch(action)
        {
            case "Reject":
                _joinRequestsRepository.UpdateJoinRequest(requestId, SquadInvitationStatus.Rejected, payload.Message);
                return Ok(new
                {
                    result = "Join request rejected"
                });
            case "Accept":
                _joinRequestsRepository.UpdateJoinRequest(requestId, SquadInvitationStatus.Accepted, payload.Message);
                await _squadsRepository.AddUserToSquad(squad.Id, joinRequest.SenderUserId);
                return Ok(new
                {
                    result = "Join request accepted. User joined."
                });
            default:
                return Problem("No action performed.");
        }
    }

    /// <summary>
    /// Endpoint that returns join request that this squad has received
    /// </summary>
    /// <param name="sessionToken">Auth token</param>
    /// <param name="squadId">Squad id</param>
    /// <param name="lastId">Last item served for pagination</param>
    /// <returns></returns>
    [HttpGet]
    [Route("/api/Squads/{squadId:guid}/JoinRequests")]
    public async Task<IActionResult> ListRequestsOfSquad(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        Guid squadId,
        string lastId = null)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new
            {
                error = "Squad not found"
            });
        }

        var userIsAdminOrMember = squad.UserId.Equals(user.Id) 
                                  || await _squadsRepository.UserBelongsToSquad(user.Id, squadId);
        if (!userIsAdminOrMember)
        {
            return Unauthorized(new
            {
                error = "Unauthorized. You cannot access join requests of this squad because you are not a member or admin"
            });
        }

        
        return Ok(await _joinRequestsRepository.GetJoinRequestsOfSquad(squad.Id, lastId));
    }

    /// <summary>
    /// Endpoint that returns the join requests the user has made
    /// </summary>
    /// <param name="sessionToken">Auth token</param>
    /// <param name="lastId">Last join request served id for pagination.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("MyJoinRequests")]
    public async Task<IActionResult> ListRequestsMadeOfUser(
        [FromHeader(Name = "sessionToken")] string sessionToken, 
        string lastId = null)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

        var joinRequests = await _joinRequestsRepository.GetJoinRequestsOfUser(user.Id, lastId);
        
        return Ok(joinRequests.Select(request => new
        {
            request,
            username = _accounts.GetUsernameFromId(request.SenderUserId),
            squadName = _squadsRepository.GetSquadName(request.SquadId)
        }));
    }
    /// <summary>
    /// Endpoint that returns the join requests the user has received in the squads this admins
    /// </summary>
    /// <param name="sessionToken">Auth token</param>
    /// <param name="lastId">Last join request served id for pagination.</param>
    /// <returns></returns>
    
    [HttpGet]
    [Route("JoinRequestsForMe")]
    public async Task<IActionResult> ListRequestsMadeForUser([FromHeader(Name = "sessionToken")] string sessionToken, 
        string lastId = null)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if(user == null) return Unauthorized("Token is not valid");

        var joinRequests = await _joinRequestsRepository.GetJoinRequestsForUser(user.Id, lastId);
        
        return Ok(joinRequests.Select(request => new
        {
            request,
            username = _accounts.GetUsernameFromId(request.SenderUserId),
            squadName = _squadsRepository.GetSquadName(request.SquadId)
        }));
    }
}
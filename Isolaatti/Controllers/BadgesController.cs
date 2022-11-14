using System.Linq;
using System.Threading.Tasks;
using Isolaatti.DTOs;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/badges")]
public class BadgesController : ControllerBase
{
    private readonly IAccounts _accounts;
    private readonly SquadJoinRequestsRepository _joinRequestsRepository;
    private readonly SquadInvitationsRepository _squadInvitationsRepository;
    private readonly SquadsRepository _squadsRepository;

    public BadgesController(IAccounts accounts, 
        SquadJoinRequestsRepository joinRequestsRepository,
        SquadsRepository squadsRepository,
        SquadInvitationsRepository squadInvitationsRepository)
    {
        _accounts = accounts;
        _joinRequestsRepository = joinRequestsRepository;
        _squadsRepository = squadsRepository;
        _squadInvitationsRepository = squadInvitationsRepository;
    }
    
    [HttpGet]
    [Route("squads")]
    public async Task<IActionResult> Index([FromHeader(Name = "sessionToken")] string sessionToken)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");
        
        var squadsOfUser = _squadsRepository.GetSquadsUserAdmins(user.Id).Select(squad => squad.Id).ToArray();
        var response = new SquadsBadgesStatusDto
        {
            UnseenInvitations = await _squadInvitationsRepository.GetUnseenInvitationsForUser(user.Id),
            UnseenRequests = await _joinRequestsRepository.GetUnseenRequestsForUser(squadsOfUser)
        };
        
        return Ok(response);
    }
}
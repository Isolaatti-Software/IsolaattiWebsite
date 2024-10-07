using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Squads.Dto;
using Isolaatti.Squads.Repository;
using Isolaatti.Utils;

using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/badges")]
public class BadgesController : ProfileRequiredController
{
    private readonly SquadJoinRequestsRepository _joinRequestsRepository;
    private readonly SquadInvitationsRepository _squadInvitationsRepository;
    private readonly SquadsRepository _squadsRepository;

    public BadgesController(SquadJoinRequestsRepository joinRequestsRepository,
        SquadsRepository squadsRepository,
        SquadInvitationsRepository squadInvitationsRepository)
    {
        _joinRequestsRepository = joinRequestsRepository;
        _squadsRepository = squadsRepository;
        _squadInvitationsRepository = squadInvitationsRepository;
    }
    
    
    [HttpGet]
    [Route("squads")]
    public async Task<IActionResult> OfSquads()
    {
        var squadsOfUser = _squadsRepository.GetSquadsUserOwns(Profile.Id).Select(squad => squad.Id).ToArray();
        var response = new SquadsBadgesStatusDto
        {
            UnseenInvitations = await _squadInvitationsRepository.GetUnseenInvitationsForUser(Profile.Id),
            UnseenRequests = await _joinRequestsRepository.GetUnseenRequestsForUser(squadsOfUser)
        };
        
        return Ok(response);
    }
}
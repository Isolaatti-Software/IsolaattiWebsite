using System.Linq;
using System.Threading.Tasks;
using Isolaatti.DTOs;
using Isolaatti.Repositories;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/badges")]
public class BadgesController : IsolaattiController
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
    
    [IsolaattiAuth]
    [HttpGet]
    [Route("squads")]
    public async Task<IActionResult> OfSquads()
    {
        var squadsOfUser = _squadsRepository.GetSquadsUserAdmins(User.Id).Select(squad => squad.Id).ToArray();
        var response = new SquadsBadgesStatusDto
        {
            UnseenInvitations = await _squadInvitationsRepository.GetUnseenInvitationsForUser(User.Id),
            UnseenRequests = await _joinRequestsRepository.GetUnseenRequestsForUser(squadsOfUser)
        };
        
        return Ok(response);
    }
}
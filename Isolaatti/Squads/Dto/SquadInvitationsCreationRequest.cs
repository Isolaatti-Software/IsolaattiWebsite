using System.Collections.Generic;

namespace Isolaatti.Squads.Dto;

public class SquadInvitationsCreationRequest
{
    public IEnumerable<int> UserIds { get; set; }
    public string Message { get; set; }
}
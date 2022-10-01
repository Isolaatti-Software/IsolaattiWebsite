using System.Collections.Generic;

namespace Isolaatti.Classes.ApiEndpointsRequestDataModels;

public class SquadInvitationsCreationRequest
{
    public IEnumerable<int> UserIds { get; set; }
    public string Message { get; set; }
}
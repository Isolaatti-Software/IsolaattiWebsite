using System;

namespace Isolaatti.Classes.ApiEndpointsRequestDataModels;

public class SquadInvitationCreationRequest
{
    public int UserId { get; set; }
    public string Message { get; set; }
}
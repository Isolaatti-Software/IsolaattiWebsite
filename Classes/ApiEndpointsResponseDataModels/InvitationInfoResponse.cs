using System;
using Isolaatti.Models.MongoDB;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels;

public class InvitationInfoResponse
{
    public SquadInvitation Invitation { get; set; }
    public string SenderName { get; set; }
    public string RecipientName { get; set; }
    public string SquadName { get; set; }
}
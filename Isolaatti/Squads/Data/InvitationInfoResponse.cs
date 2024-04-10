using Isolaatti.Database.MongoDb;

namespace Isolaatti.Squads.Data;

public class InvitationInfoResponse
{
    public SquadInvitation Invitation { get; set; }
    public string SenderName { get; set; }
    public string RecipientName { get; set; }
    public string SquadName { get; set; }
}
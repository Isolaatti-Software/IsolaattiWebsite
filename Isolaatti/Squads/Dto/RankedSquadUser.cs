using Isolaatti.Accounts.Data;

namespace Isolaatti.Squads.Dto;

public class RankedSquadUser
{
    public UserFeedDto User { get; set; }
    public double Ranking { get; set; }
}
using Isolaatti.Accounts.Data;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;

namespace Isolaatti.DTOs;

public class RankedSquadUser
{
    public UserFeedDto User { get; set; }
    public double Ranking { get; set; }
}
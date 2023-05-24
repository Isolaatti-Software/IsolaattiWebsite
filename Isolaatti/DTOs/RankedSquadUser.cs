using Isolaatti.Classes.ApiEndpointsResponseDataModels;

namespace Isolaatti.DTOs;

public class RankedSquadUser
{
    public UserFeed User { get; set; }
    public double Ranking { get; set; }
}
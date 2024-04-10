using System.Collections.Generic;

namespace Isolaatti.Squads.Dto;

public class UserSearchFeed
{
    public List<RankedSquadUser> Users { get; set; }
    public int? LastId { get; set; }
}
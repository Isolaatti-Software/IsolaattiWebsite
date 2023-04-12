using System.Collections.Generic;

namespace Isolaatti.DTOs;

public class UserSearchFeed
{
    public List<RankedSquadUser> Users { get; set; }
    public int? LastId { get; set; }
}
using Isolaatti.Enums;
using Isolaatti.Squads.Data;

namespace Isolaatti.Squads.Dto;

public class SquadCreationResponse
{
    public SquadCreationResult CreationResult { get; set; }
    public SquadListItemDto? Squad { get; set; }
}
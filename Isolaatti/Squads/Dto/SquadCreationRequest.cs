using Isolaatti.Enums;

namespace Isolaatti.Squads.Dto;

public class SquadCreationRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ExtendedDescription { get; set; }
    public SquadPrivacy Privacy { get; set; }
}
using System.Text.Json.Serialization;

namespace Isolaatti.Squads.Dto;

public class SquadUpdateInfoRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ExtendedDescription { get; set; }
}
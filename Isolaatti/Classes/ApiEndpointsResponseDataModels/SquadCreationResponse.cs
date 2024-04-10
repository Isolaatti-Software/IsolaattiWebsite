using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Squads.Data;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels;

public class SquadCreationResponse
{
    public SquadCreationResult CreationResult { get; set; }
    public Squad? Squad { get; set; }
}
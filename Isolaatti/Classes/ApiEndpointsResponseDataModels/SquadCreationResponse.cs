using Isolaatti.Enums;
using Isolaatti.Models;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels;

public class SquadCreationResponse
{
    public SquadCreationResult CreationResult { get; set; }
    public Squad? Squad { get; set; }
}
using System.Text.Json.Serialization;

namespace Isolaatti.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SquadUpdateResult
{
    Success, ValidationErrors, Error, SquadDoesNotExist
}
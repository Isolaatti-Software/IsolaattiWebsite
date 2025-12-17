
using System.Text.Json.Serialization;

namespace Isolaatti.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SquadUserRole
{
    Outsider = 0, User = 1, Owner = 2
}
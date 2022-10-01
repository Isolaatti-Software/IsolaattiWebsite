using System;
using System.Text.Json.Serialization;
using Isolaatti.Enums;

namespace Isolaatti.Models;

public class SquadUser
{
    public Guid Id { get; set; }
    public Guid SquadId { get; set; }
    public int UserId { get; set; }
    public SquadUserRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    
    [JsonIgnore] public User User { get; set; }
    [JsonIgnore] public Squad Squad { get; set; }
}
using System;
using System.Collections.Generic;
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
    public DateTime LastInteractionDateTime { get; set; }
    public double Ranking { get; set; }
    public List<string> Permissions { get; set; }
    
    [JsonIgnore] public virtual User User { get; set; }
    [JsonIgnore] public virtual Squad Squad { get; set; }
}
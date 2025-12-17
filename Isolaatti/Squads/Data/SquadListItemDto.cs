using System;
using Isolaatti.Enums;

namespace Isolaatti.Squads.Data;

public class SquadListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ExtendedDescription { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid? ImageId { get; set; }
    public string? ImageUrl { get; set; }
    public bool PlatformGenerated { get; set; }
    public string? Keywords { get; set; }
    public SquadUserRole? Membership { get; set; }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Enums;

namespace Isolaatti.Squads.Data;

public class Squad
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ExtendedDescription { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid? ImageId { get; set; }
    public bool PlatformGenerated { get; set; }
    public string? Keywords { get; set; }
    public virtual ICollection<SquadUser> SquadUsers { get; set; }
} 
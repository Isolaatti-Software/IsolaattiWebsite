using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Isolaatti.Enums;

namespace Isolaatti.Models;

public class Squad
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ExtendedDescription { get; set; }
    public int UserId { get; set; }
    public SquadPrivacy Privacy { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid? ImageId { get; set; }
}
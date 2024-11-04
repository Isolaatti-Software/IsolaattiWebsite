using System;
using System.Text.Json.Serialization;
using Isolaatti.Posting.Data;

namespace Isolaatti.Images.Data;

public class Image
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    
    public Guid? SquadId { get; set; }
    public string IdOnFirebase { get; set; }
    public bool Outstanding { get; set; }
    
    public string? Username { get; set; }
    public long? PostId { get; set; }

    [JsonIgnore] public virtual Post? Post { get;  }
}
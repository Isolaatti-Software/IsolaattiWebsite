using System;
using System.Text.Json.Serialization;
using Isolaatti.Accounts.Data.Entity;

namespace Isolaatti.MediaStreaming.Entity;

public class StreamingStationEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int UserId { get; set; }
    [JsonIgnore]
    public string? KeyHash { get; set; }
    
    public bool IsLive { get; set; }
    
    [JsonIgnore]
    public User User { get; set; }
}
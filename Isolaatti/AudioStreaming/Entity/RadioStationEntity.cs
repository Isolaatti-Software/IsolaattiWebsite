using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Isolaatti.Accounts.Data.Entity;

namespace Isolaatti.AudioStreaming.Entity;

public class RadioStationEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int UserId { get; set; }
    [JsonIgnore]
    public string KeyHash { get; set; }
    [JsonIgnore]
    public User User { get; set; }
}
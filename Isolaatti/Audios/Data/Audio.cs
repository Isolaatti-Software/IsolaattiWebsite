using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Isolaatti.Posting.Data;

namespace Isolaatti.Audios.Data;

public class Audio
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    
    
    [JsonIgnore]
    public virtual Post? Post { get; set; }
    

    public DateTime CreationTime { get; set; } = DateTime.Now.ToUniversalTime();

    [JsonIgnore]
    public string FirestoreObjectPath { get; set; }
    
    public int DurationSeconds { get; set; }
}
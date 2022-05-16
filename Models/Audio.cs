using System;

namespace isolaatti_API.Models;

public class Audio
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
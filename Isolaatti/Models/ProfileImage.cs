using System;

namespace Isolaatti.Models;

public class ProfileImage
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public string ImageData { get; set; }
}
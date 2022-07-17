using System;

namespace isolaatti_API.Models;

public class ProfileImage
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public string ImageData { get; set; }
}
using System;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels;

public class UserFeed
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Guid? ImageId { get; set; }
}
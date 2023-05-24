using System;

namespace Isolaatti.Models;

public class TrackingUserInteraction
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public string AudioId { get; set; }
    public int AudioDuration { get; set; }
}
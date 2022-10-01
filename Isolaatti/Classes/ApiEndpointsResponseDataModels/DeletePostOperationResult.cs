using System;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels;

public class DeletePostOperationResult
{
    public long PostId { get; set; }
    public bool Success { get; set; }
    public DateTime OperationTime { get; set; }
}
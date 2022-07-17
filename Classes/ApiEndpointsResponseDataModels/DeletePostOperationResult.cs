using System;

namespace isolaatti_API.Classes.ApiEndpointsResponseDataModels;

public class DeletePostOperationResult
{
    public long PostId { get; set; }
    public bool Success { get; set; }
    public DateTime OperationTime { get; set; }
}
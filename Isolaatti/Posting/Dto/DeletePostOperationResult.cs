using System;

namespace Isolaatti.Posting.Dto;

public class DeletePostOperationResult
{
    public long PostId { get; set; }
    public bool Success { get; set; }
    public DateTime OperationTime { get; set; }
}
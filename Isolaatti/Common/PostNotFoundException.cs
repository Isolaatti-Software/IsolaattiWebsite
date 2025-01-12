using System;

namespace Isolaatti.Common;

public class PostNotFoundException(long id) : Exception
{
    public override string Message { get; } = "Post with id $id was not found";
}
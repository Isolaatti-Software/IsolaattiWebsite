using System;

namespace Isolaatti.Common;

public class UnauthorizedUserException(int userId) : Exception
{
    public override string Message { get; } = $"User with id {userId} is not authorized";
}
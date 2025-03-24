using System;

namespace Isolaatti.Common;

public class UserNotFoundException(int userId) : Exception
{
    public override string Message { get; } = "User with id $userId was not found";
}
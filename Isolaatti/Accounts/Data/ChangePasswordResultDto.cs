namespace Isolaatti.Accounts.Data;

public record ChangePasswordResultDto(bool Success, string? Reason = null)
{
    public const string ReasonUserDoesNotExist = "user_does_not_exist";
    public const string ReasonOldPasswordMismatch = "old_password_mismatch";
    public const string ReasonNewPasswordInvalid = "new_password_invalid";
}
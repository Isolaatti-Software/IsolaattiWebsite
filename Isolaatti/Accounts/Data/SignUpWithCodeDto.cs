using Isolaatti.Enums;

namespace Isolaatti.Accounts.Data;

public class SignUpWithCodeDto
{
    public AccountMakingResult AccountMakingResult { get; set; }
    public string? Session { get; set; }
}
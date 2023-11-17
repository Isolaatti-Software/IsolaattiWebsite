using Isolaatti.Enums;

namespace Isolaatti.Accounts.Service;

public class AccountMakeResult
{
    public AccountMakingResult AccountMakingResult { get; set; }
    public int? UserId { get; set; }
}
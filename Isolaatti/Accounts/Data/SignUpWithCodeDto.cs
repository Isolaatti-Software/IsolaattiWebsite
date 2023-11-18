using Isolaatti.Classes.ApiEndpointsResponseDataModels;

namespace Isolaatti.Accounts.Data;

public class SignUpWithCodeDto
{
    public string AccountMakingResult { get; set; }
    public SessionToken? Session { get; set; }
}
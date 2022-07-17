using System.Threading.Tasks;
using isolaatti_API.Enums;
using isolaatti_API.Models;

namespace isolaatti_API.Services;

public interface IAccounts
{
    Task<AccountMakingResult> MakeAccountAsync(string username, string email, string password);
    Task<bool> IsUserEmailVerified(int userId);
    Task<bool> ChangeAPassword(int userId, string currentPassword, string newPassword);
    Task<SessionToken> CreateNewToken(int userId, string plainTextPassword);
    Task<User> ValidateToken(string token);
    Task RemoveAToken(string token);
    Task RemoveAllUsersTokens(int userId);
    Task MakeAccountFromGoogleAccount(string accessToken);
    Task<SessionToken> CreateTokenForGoogleUser(string accessToken);
    Task SendJustLoginEmail(string email, string name, string ipAddress);
}
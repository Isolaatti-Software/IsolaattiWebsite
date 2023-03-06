using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.Authentication;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;

namespace Isolaatti.Services;

public interface IAccounts
{
    Task<AccountMakingResult> MakeAccountAsync(string username, string email, string password);
    Task<bool> IsUserEmailVerified(int userId);
    Task<bool> ChangeAPassword(int userId, string currentPassword, string newPassword);
    Task<AuthenticationTokenSerializable> CreateNewToken(int userId, string plainTextPassword);
    Task<User> ValidateToken(string token);
    string GetUsernameFromId(int userId);
    Task RemoveAToken(int userId, string id);
    Task RemoveAllUsersTokens(int userId);
    Task MakeAccountFromGoogleAccount(string accessToken);
    Task<AuthenticationTokenSerializable> CreateTokenForGoogleUser(string accessToken);
    Task SendJustLoginEmail(string email, string name, string ipAddress);
    IEnumerable<AuthToken> GetTokenOfUser(int userId);
    string GetIpAddress();
}
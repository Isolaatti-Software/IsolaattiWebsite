using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.DTOs;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;

namespace Isolaatti.Services;

public interface IAccounts
{
    Task<AccountMakingResult> MakeAccountAsync(string username, string displayName, string email, string password);
    Task<bool> IsUserEmailVerified(int userId);
    Task<bool> ChangeAPassword(int userId, string currentPassword, string newPassword);
    Task<string> CreateNewSession(int userId, string plainTextPassword);
    Task<User> ValidateSession(SessionDto sessionDto);

    Task<bool> RemoveSession(SessionDto sessionDto);
    string GetUsernameFromId(int userId);
    Task MakeAccountFromGoogleAccount(string accessToken);
    Task<string> CreateTokenForGoogleUser(string accessToken);
    void SendJustLoginEmail(string email, string name, string ipAddress);
    IEnumerable<Session> GetSessionsOfUser(int userId);
    string GetIpAddress();
    void RemoveSessionCookie();
    Task<Session> CurrentSession();
}
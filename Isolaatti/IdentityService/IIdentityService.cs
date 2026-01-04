using System.Threading.Tasks;

namespace Isolaatti.IdentityService;

public interface IIdentityService
{
    public Task<bool> UserExists(string email);
    
    public Task<string> CreateUser(string username, string email, string password, string firstName, string lastName);
}
using System.Threading.Tasks;

namespace Isolaatti.Messenger;

public interface IMessagingServer
{
    public Task<bool> CreateUser(int userId, string username, string name, string email, string password);
}
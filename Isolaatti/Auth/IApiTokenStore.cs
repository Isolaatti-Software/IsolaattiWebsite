using System.Threading.Tasks;

namespace Isolaatti.Auth;

public interface IApiTokenStore
{
    Task<ApiToken> GetByHashAsync(string tokenHash);
}
using System.Threading.Tasks;

namespace Isolaatti.Auth;

public class ApiTokenStore : IApiTokenStore
{
    public Task<ApiToken> GetByHashAsync(string tokenHash)
    {
        throw new System.NotImplementedException();
    }
}
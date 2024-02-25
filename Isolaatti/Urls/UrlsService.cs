using Isolaatti.Config;
using Microsoft.Extensions.Options;

namespace Isolaatti.Urls;

public class UrlsService
{
    private readonly IOptions<HostConfig> _hostOptions;

    public UrlsService(IOptions<HostConfig> hostOptions)
    {
        _hostOptions = hostOptions;
    }

    public string PostUrl(long postId)
    {
        return $"{_hostOptions.Value.BaseUrl}/pub/{postId}";
    }
    
    public string ProfilePictureUrl(int userId)
    {
        return $"{_hostOptions.Value.BaseUrl}/api/images/profile_image/of_user/{userId}?mode=small";
    }
}
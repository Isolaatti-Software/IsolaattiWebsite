using Microsoft.AspNetCore.Http;

namespace Isolaatti.Utils
{
    public class UrlGenerators
    {
        public static string GenerateProfilePictureUrl(int userId, string token)
        {
            return $"/api/images/profile_image/of_user/{userId}?mode=small";
        }
    }
}
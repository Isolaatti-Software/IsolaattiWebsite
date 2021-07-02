using isolaatti_API.Models;

namespace isolaatti_API.Utils
{
    public class UrlGenerators
    {
        public static string GenerateProfilePictureUrl(int userId, string sessionToken)
        {
            return $"/api/Fetch/GetUserProfileImage?userId={userId}&sessionToken={sessionToken}";
        }
    }
}
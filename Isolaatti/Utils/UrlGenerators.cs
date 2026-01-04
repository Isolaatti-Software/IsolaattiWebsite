namespace Isolaatti.Utils
{
    public class UrlGenerators
    {
        public static string GenerateProfilePictureUrl(int userId)
        {
            return $"/api/images/profile_image/of_user/{userId}?mode=small";
        }
    }
}
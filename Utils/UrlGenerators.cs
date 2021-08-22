using isolaatti_API.Models;
using Microsoft.AspNetCore.Http;

namespace isolaatti_API.Utils
{
    public class UrlGenerators
    {
        public static string GenerateProfilePictureUrl(int userId, string sessionToken, HttpRequest request = null)
        {
            string res = "";
            if (request != null)
            {
                var protocol = request.IsHttps ? "https://" : "http://";
                res += $"{protocol}{request.HttpContext.Request.Host.Value}";
            }

            res += $"/api/Fetch/GetUserProfileImage?userId={userId}&sessionToken={sessionToken}";
            return res;
        }
    }
}
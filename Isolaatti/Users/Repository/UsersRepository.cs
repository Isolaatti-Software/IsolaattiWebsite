using Isolaatti.Models;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Isolaatti.Users.Repository
{
    public class UsersRepository
    {
        private readonly DbContextApp _db;
        private readonly IDistributedCache _cache;

        public UsersRepository(DbContextApp db, IDistributedCache cache)
        {
            _db = db;
            _cache = cache;
        }

        private const string UsernamePrefix = "user_names";
        private const string FollowersCountPrefix = "follower_counts";
        private const string UserImageIdPrefix = "user_profile_images";

        private static string GetKeyForUsername(int userId)
        {
            return $"{UsernamePrefix}.{userId}";
        }

        private static string GetKeyForFollowersCount(int userId)
        {
            return $"{FollowersCountPrefix}.{userId}";
        }

        private static string GetKeyForUserImageId(int userId)
        {
            return $"{UserImageIdPrefix}.{userId}";
        }

        public string GetUsernameById(int userId)
        {
            string value = _cache.GetString(GetKeyForUsername(userId));

            if (value == null)
            {
                value = _db.Users.Where(u => u.Id == userId).Select(u => u.Name).FirstOrDefault();
                if (value == null)
                {
                    return null;
                }

                _cache.SetString(GetKeyForUsername(userId), value);
            }

            return value;
        }

        public int GetFollowersCount(int userId)
        {
            string value = _cache.GetString(GetKeyForFollowersCount(userId));

            if (value == null)
            {
                value = _db.FollowerRelations.Count(fr => fr.TargetUserId == userId).ToString();

                _cache.SetString(GetKeyForFollowersCount(userId), value);
            }

            return Convert.ToInt32(value);

        }

        public string GetUserImageId(int userId)
        {
            string value = _cache.GetString(GetKeyForUserImageId(userId));

            if (value == null)
            {
                value = _db.Users.Where(u => u.Id == userId).Select(u => u.ProfileImageId).FirstOrDefault();
            }

            return value;
        }
    }
}

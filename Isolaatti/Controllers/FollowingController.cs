using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Isolaatti.Notifications.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/Following")]
    public class FollowingController : IsolaattiController
    {
        private readonly DbContextApp Db;
        private readonly IDistributedCache _cache;
        private readonly NotificationsService _notificationsService;

        public FollowingController(DbContextApp dbContextApp, IDistributedCache cache, NotificationsService notificationsService)
        {
            Db = dbContextApp;
            _cache = cache;
            _notificationsService = notificationsService;
        }

        private string FollowStateCacheKey(int userId, int targetUserId)
        {
            return $"user_{userId}_follows_${targetUserId}";
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("Follow")]
        public async Task<IActionResult> Index(SingleIdentification identification)
        {
            var userToFollow = await Db.Users.FindAsync(Convert.ToInt32(identification.Id));
            if (userToFollow == null) return NotFound("User to follow was not found");

            // create new relation only if there is not one already
            if (await Db.FollowerRelations.AnyAsync(rel =>
                    rel.UserId.Equals(User.Id) && rel.TargetUserId.Equals(userToFollow.Id)))
            {
                return Unauthorized("user already followed");
            }

            var followerRelation = new FollowerRelation
            {
                UserId = User.Id,
                TargetUserId = userToFollow.Id
            };

            Db.FollowerRelations.Add(followerRelation);
            await Db.SaveChangesAsync();

            _cache.SetString(FollowStateCacheKey(User.Id, userToFollow.Id), "1");

            await _notificationsService.InsertNewFollowerNotification(followerRelation);
            return Ok(await GetFollowingInfo(userToFollow.Id));
        }

        [IsolaattiAuth]
        [Route("Unfollow")]
        [HttpPost]
        public async Task<IActionResult> Unfollow(SingleIdentification identification)
        {
            var userToUnfollow = await Db.Users.FindAsync(Convert.ToInt32(identification.Id));
            if (userToUnfollow == null) return NotFound("User to unfollow was not found");

            try
            {
                // finds and removes follower relation
                var followerRelation = await Db.FollowerRelations.SingleAsync(relation =>
                    relation.UserId.Equals(User.Id) && relation.TargetUserId.Equals(userToUnfollow.Id));
                Db.FollowerRelations.Remove(followerRelation);

                await Db.SaveChangesAsync();
                _cache.SetString(FollowStateCacheKey(User.Id, userToUnfollow.Id), "0");
                
                return Ok(await GetFollowingInfo(userToUnfollow.Id));
            }
            catch (InvalidOperationException)
            {
                return Unauthorized("cannot unfollow user, not followed");
            }
        }

        private bool UserFollowsUser(int userId, int targetUserId)
        {
            var rawValue = _cache.GetString(FollowStateCacheKey(userId, targetUserId));
            if(rawValue != null)
            {
                if(rawValue.Equals("1"))
                {
                    return true;
                } 
                else if(rawValue.Equals("0"))
                {
                    return false;
                }
            }
            var value = Db.FollowerRelations.Any(fr => fr.UserId == userId && fr.TargetUserId == targetUserId);

            _cache.SetString(FollowStateCacheKey(userId, targetUserId), value ? "1" : "0");

            return value;
        }

        [IsolaattiAuth]
        [Route("FollowingsOf/{userId:int}")]
        [HttpGet]
        public IActionResult Following(int userId, int lastId)
        {
            var listOfFollowing =
                (from _user in Db.Users
                 from _relation in Db.FollowerRelations
                 where _relation.UserId == userId && _user.Id == _relation.TargetUserId && _relation.TargetUserId > lastId
                 select new UserFeedDto
                 {
                     Id = _relation.TargetUserId,
                     Name = _user.Name,
                     ImageId = _user.ProfileImageId,
                     Following = false

                 })
                    .OrderBy(u => u.Id)
                    .Take(10)
                    .ToList().Select(u => { u.Following = UserFollowsUser(User.Id, u.Id); return u; });

            return Ok(listOfFollowing);
        }

        [IsolaattiAuth]
        [Route("FollowersOf/{userId:int}")]
        [HttpGet]
        public IActionResult Followers(int userId, int lastId)
        {
            var listOfFollowers =
                (from _user in Db.Users
                    from _relation in Db.FollowerRelations
                    where _relation.TargetUserId == userId && _relation.UserId == _user.Id && _relation.UserId > lastId
                    select new UserFeedDto
                    {
                        Id = _relation.UserId,
                        Name = _user.Name,
                        ImageId = _user.ProfileImageId,
                        Following = true
                    })
                    .OrderBy(u => u.Id)
                    .Take(10)
                    .ToList().Select(u => { u.Following = UserFollowsUser(u.Id, User.Id); return u; });
            return Ok(listOfFollowers);
        }

        private async Task<FollowDto> GetFollowingInfo(int userId)
        {
            return new FollowDto() 
            {
                ThisUserIsFollowingMe = await Db.FollowerRelations.AnyAsync(fr => fr.TargetUserId == User.Id && fr.UserId == userId),
                FollowingThisUser = await Db.FollowerRelations.AnyAsync(fr => fr.UserId == User.Id && fr.TargetUserId == userId)
            };
        }
    }
}
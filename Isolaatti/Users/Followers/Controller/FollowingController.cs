using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Models;
using Isolaatti.Notifications.Services;
using Isolaatti.Users.Followers.Dto;
using Isolaatti.Users.Repository;
using Isolaatti.Utils;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Users.Followers.Controller
{
    [ApiController]
    [Route("/api/Following")]
    public class FollowingController : ProfileRequiredController
    {
        private readonly DbContextApp Db;
        private readonly UsersRepository _usersRepository;
        private readonly NotificationsService _notificationsService;

        public FollowingController(DbContextApp dbContextApp, UsersRepository usersRepository, NotificationsService notificationsService)
        {
            Db = dbContextApp;
            _usersRepository = usersRepository;
            _notificationsService = notificationsService;
        }
        
        
        [HttpPost]
        [Route("Follow")]
        public async Task<IActionResult> Index(SingleIdentification identification)
        {
            var userToFollow = await Db.Users.FindAsync(Convert.ToInt32(identification.Id));
            if (userToFollow == null) return NotFound("User to follow was not found");

            // create new relation only if there is not one already
            if (await Db.FollowerRelations.AnyAsync(rel =>
                    rel.ProfileId.Equals(Profile.Id) && rel.TargetProfileId.Equals(userToFollow.Id)))
            {
                return Unauthorized("user already followed");
            }

            var followerRelation = new FollowerRelation
            {
                ProfileId = Profile.Id,
                TargetProfileId = userToFollow.Id
            };

            Db.FollowerRelations.Add(followerRelation);
            await Db.SaveChangesAsync();

            _usersRepository.SetUserFollowsUser(Profile.Id, userToFollow.Id, true);

            await _notificationsService.InsertNewFollowerNotification(followerRelation);
            return Ok(await GetFollowingInfo(userToFollow.Id));
        }

        
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
                    relation.ProfileId.Equals(Profile.Id) && relation.TargetProfileId.Equals(userToUnfollow.Id));
                Db.FollowerRelations.Remove(followerRelation);

                await Db.SaveChangesAsync();
                _usersRepository.SetUserFollowsUser(Profile.Id, userToUnfollow.Id, false);
                
                return Ok(await GetFollowingInfo(userToUnfollow.Id));
            }
            catch (InvalidOperationException)
            {
                return Unauthorized("cannot unfollow user, not followed");
            }
        }

        

        
        [Route("FollowingsOf/{userId:int}")]
        [HttpGet]
        public IActionResult Following(int userId, int lastId)
        {
            var listOfFollowing =
                (from _user in Db.Users
                 from _relation in Db.FollowerRelations
                 where _relation.ProfileId == userId && _user.Id == _relation.TargetProfileId && _relation.TargetProfileId > lastId
                 select new UserFeedDto
                 {
                     Id = _relation.TargetProfileId,
                     Name = _user.Name,
                     ImageId = _user.ProfileImageId,
                     Following = false

                 })
                    .OrderBy(u => u.Id)
                    .Take(10)
                    .ToList().Select(u => { u.Following = _usersRepository.GetUserFollowsUser(Profile.Id, u.Id); return u; });

            return Ok(listOfFollowing);
        }

        
        [Route("FollowersOf/{userId:int}")]
        [HttpGet]
        public IActionResult Followers(int userId, int lastId)
        {
            var listOfFollowers =
                (from _user in Db.Users
                    from _relation in Db.FollowerRelations
                    where _relation.TargetProfileId == userId && _relation.ProfileId == _user.Id && _relation.ProfileId > lastId
                    select new UserFeedDto
                    {
                        Id = _relation.ProfileId,
                        Name = _user.Name,
                        ImageId = _user.ProfileImageId,
                        Following = true
                    })
                    .OrderBy(u => u.Id)
                    .Take(10)
                    .ToList().Select(u => { u.Following = _usersRepository.GetUserFollowsUser(Profile.Id, u.Id); return u; });
            return Ok(listOfFollowers);
        }

        private async Task<FollowDto> GetFollowingInfo(int userId)
        {
            return new FollowDto() 
            {
                ThisUserIsFollowingMe = await Db.FollowerRelations.AnyAsync(fr => fr.TargetProfileId == Profile.Id && fr.ProfileId == userId),
                FollowingThisUser = await Db.FollowerRelations.AnyAsync(fr => fr.ProfileId == Profile.Id && fr.TargetProfileId == userId)
            };
        }
    }
}
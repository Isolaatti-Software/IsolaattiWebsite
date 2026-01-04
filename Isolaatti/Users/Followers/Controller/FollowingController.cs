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
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Users.Followers.Controller
{
    [ApiController]
    [Route("/api/Following")]
    public class FollowingController : IsolaattiController
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
        
        [Authorize]
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

            _usersRepository.SetUserFollowsUser(User.Id, userToFollow.Id, true);

            await _notificationsService.InsertNewFollowerNotification(followerRelation);
            return Ok(await GetFollowingInfo(userToFollow.Id));
        }

        [Authorize]
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
                _usersRepository.SetUserFollowsUser(User.Id, userToUnfollow.Id, false);
                
                return Ok(await GetFollowingInfo(userToUnfollow.Id));
            }
            catch (InvalidOperationException)
            {
                return Unauthorized("cannot unfollow user, not followed");
            }
        }

        

        [Authorize]
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
                    .ToList().Select(u => { u.Following = _usersRepository.GetUserFollowsUser(User.Id, u.Id); return u; });

            return Ok(listOfFollowing);
        }

        [Authorize]
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
                    .ToList().Select(u => { u.Following = _usersRepository.GetUserFollowsUser(User.Id, u.Id); return u; });
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
using System;
using System.Linq;
using System.Threading.Tasks;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.Models;
using isolaatti_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/Following")]
    public class FollowingController : ControllerBase
    {
        private readonly DbContextApp Db;
        private readonly IAccounts _accounts;

        public FollowingController(DbContextApp dbContextApp, IAccounts accounts)
        {
            Db = dbContextApp;
            _accounts = accounts;
        }

        [HttpPost]
        [Route("Follow")]
        public async Task<IActionResult> Index([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var userToFollow = await Db.Users.FindAsync(Convert.ToInt32(identification.Id));
            if (userToFollow == null) return NotFound("User to follow was not found");

            // create new relation only if there is not one already
            if (await Db.FollowerRelations.AnyAsync(rel =>
                    rel.UserId.Equals(user.Id) && rel.TargetUserId.Equals(userToFollow.Id)))
            {
                return Unauthorized("user already followed");
            }

            var followerRelation = new FollowerRelation
            {
                UserId = user.Id,
                TargetUserId = userToFollow.Id
            };

            Db.FollowerRelations.Add(followerRelation);

            await Db.SaveChangesAsync();

            // update number of followings and followers of involved users
            user.NumberOfFollowing = Db.FollowerRelations.Count(relation => relation.UserId.Equals(user.Id));
            userToFollow.NumberOfFollowers =
                Db.FollowerRelations.Count(relation => relation.TargetUserId.Equals(userToFollow.Id));

            Db.Users.Update(user);
            Db.Users.Update(userToFollow);
            await Db.SaveChangesAsync();
            return Ok("Followers updated!");
        }

        [Route("Unfollow")]
        [HttpPost]
        public async Task<IActionResult> Unfollow([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var userToUnfollow = await Db.Users.FindAsync(Convert.ToInt32(identification.Id));
            if (userToUnfollow == null) return NotFound("User to unfollow was not found");

            try
            {
                // finds and removes follower relation
                var followerRelation = await Db.FollowerRelations.SingleAsync(relation =>
                    relation.UserId.Equals(user.Id) && relation.TargetUserId.Equals(userToUnfollow.Id));
                Db.FollowerRelations.Remove(followerRelation);

                await Db.SaveChangesAsync();

                // update number of followings and followers of involved users
                user.NumberOfFollowing = Db.FollowerRelations.Count(relation => relation.UserId.Equals(user.Id));
                userToUnfollow.NumberOfFollowers =
                    await Db.FollowerRelations.CountAsync(relation => relation.TargetUserId.Equals(userToUnfollow.Id));
                Db.Users.Update(user);
                Db.Users.Update(userToUnfollow);
                await Db.SaveChangesAsync();
                return Ok("Followers updated!");
            }
            catch (InvalidOperationException)
            {
                return Unauthorized("cannot unfollow user, not followed");
            }
        }

        [Route("FollowingsOf/{userId:int}")]
        [HttpGet]
        public async Task<IActionResult> Following([FromHeader(Name = "sessionToken")] string sessionToken, int userId)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var listOfFollowing =
                (from _user in Db.Users
                    from _relation in Db.FollowerRelations
                    where _relation.UserId == userId && _user.Id == _relation.TargetUserId
                    select new
                    {
                        Id = _relation.TargetUserId,
                        Name = _user.Name,
                        ImageUrl = Utils.UrlGenerators.GenerateProfilePictureUrl(_relation.TargetUserId, sessionToken,
                            Request)
                    }).ToList();

            return Ok(listOfFollowing);
        }

        [Route("FollowersOf/{userId:int}")]
        [HttpGet]
        public async Task<IActionResult> Followers([FromHeader(Name = "sessionToken")] string sessionToken, int userId)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var listOfFollowers =
                (from _user in Db.Users
                    from _relation in Db.FollowerRelations
                    where _relation.TargetUserId == userId && _relation.UserId == _user.Id
                    select new
                    {
                        Id = _relation.UserId,
                        Name = _user.Name,
                        ImageUrl = Utils.UrlGenerators.GenerateProfilePictureUrl(_relation.UserId, sessionToken,
                            Request)
                    }).ToList();
            return Ok(listOfFollowers);
        }
    }
}
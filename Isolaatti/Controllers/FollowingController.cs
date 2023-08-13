using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Models;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/Following")]
    public class FollowingController : IsolaattiController
    {
        private readonly DbContextApp Db;

        public FollowingController(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
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
            
            return Ok("Followers updated!");
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
                
                return Ok("Followers updated!");
            }
            catch (InvalidOperationException)
            {
                return Unauthorized("cannot unfollow user, not followed");
            }
        }

        [IsolaattiAuth]
        [Route("FollowingsOf/{userId:int}")]
        [HttpGet]
        public async Task<IActionResult> Following(int userId, int lastId)
        {
            var listOfFollowing =
                (from _user in Db.Users
                    from _relation in Db.FollowerRelations
                    where _relation.UserId == userId && _user.Id == _relation.TargetUserId && _relation.TargetUserId > lastId
                    select new UserFeed
                    {
                        Id = _relation.TargetUserId, 
                        Name = _user.Name,
                        ImageId = _user.ProfileImageId
                    })
                    .OrderBy(u => u.Id)
                    .Take(10)
                    .ToListAsync();

            return Ok(await listOfFollowing);
        }

        [IsolaattiAuth]
        [Route("FollowersOf/{userId:int}")]
        [HttpGet]
        public async Task<IActionResult> Followers(int userId, int lastId)
        {
            var listOfFollowers =
                (from _user in Db.Users
                    from _relation in Db.FollowerRelations
                    where _relation.TargetUserId == userId && _relation.UserId == _user.Id && _relation.UserId > lastId
                    select new UserFeed
                    {
                        Id = _relation.UserId,
                        Name = _user.Name,
                        ImageId = _user.ProfileImageId
                    })
                    .OrderBy(u => u.Id)
                    .Take(10)
                    .ToListAsync();
            return Ok(await listOfFollowers);
        }
    }
}
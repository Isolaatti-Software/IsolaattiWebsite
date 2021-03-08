using System.Collections.Generic;
using System.Text.Json;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Follow : Controller
    {
        private readonly DbContextApp Db;

        public Follow(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        
        [HttpPost]
        public IActionResult Index([FromForm] int userId, [FromForm] string password, [FromForm] int userToFollowId)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User not found");
            if (!user.Password.Equals(password)) return Unauthorized("Password is wrong");

            var userToFollow = Db.Users.Find(userToFollowId);
            if (userToFollow == null) return NotFound("User to follow was not found");

            // update the following list of user to add the "user to follow"
            var usersUserIsFollowing = JsonSerializer.Deserialize<List<int>>(user.FollowingIdsJson);
            if (!usersUserIsFollowing.Contains(userToFollow.Id))
            {
                usersUserIsFollowing.Add(userToFollow.Id);
                user.FollowingIdsJson = JsonSerializer.Serialize(usersUserIsFollowing);
            }
            
            // update the followers list of the "user to follow" to add a new follower (the user)
            var followersOfFollowed = JsonSerializer.Deserialize<List<int>>(userToFollow.FollowersIdsJson);
            if (!followersOfFollowed.Contains(user.Id))
            {
                followersOfFollowed.Add(user.Id);
                userToFollow.FollowersIdsJson = JsonSerializer.Serialize(followersOfFollowed);
            }
            
            Db.Users.Update(user);
            Db.Users.Update(userToFollow);
            Db.SaveChanges();
            return Ok("Followers updated!");
        }
        
        [Route("Unfollow")]
        [HttpPost]
        public IActionResult Unfollow([FromForm] int userId, [FromForm] string password, [FromForm] int userToUnfollowId)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User not found");
            if (!user.Password.Equals(password)) return Unauthorized("Password is wrong");
            
            var userToUnfollow = Db.Users.Find(userToUnfollowId);
            if (userToUnfollow == null) return NotFound("User to unfollow was not found");
            
            // update the following list of user to remove the "user to unfollow"
            var usersUserIsFollowing = JsonSerializer.Deserialize<List<int>>(user.FollowingIdsJson);
            usersUserIsFollowing.Remove(userToUnfollow.Id);
            user.FollowingIdsJson = JsonSerializer.Serialize(usersUserIsFollowing);
            
            // update the followers list of the "user to follow" to remove the follower (the user)
            var followersOfFollowed = JsonSerializer.Deserialize<List<int>>(userToUnfollow.FollowersIdsJson);
            followersOfFollowed.Remove(user.Id);
            userToUnfollow.FollowersIdsJson = JsonSerializer.Serialize(followersOfFollowed);
            
            Db.Users.Update(user);
            Db.Users.Update(userToUnfollow);
            Db.SaveChanges();
            return Ok("Followers updated!");
        }

        [Route("Following")]
        [HttpPost]
        public IActionResult Following([FromForm] int userId, [FromForm] string password)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User not found");
            if (!user.Password.Equals(password)) return Unauthorized("Password is wrong");
            return Ok(JsonSerializer.Serialize(user.FollowingIdsJson));
        }

        [Route("Followers")]
        [HttpPost]
        public IActionResult Followers([FromForm] int userId, [FromForm] string password)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User not found");
            if (!user.Password.Equals(password)) return Unauthorized("Password is wrong");
            return Ok(JsonSerializer.Serialize(user.FollowingIdsJson));
        }
    }
}
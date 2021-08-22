using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using isolaatti_API.Classes;
using isolaatti_API.Hubs;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Follow : Controller
    {
        private readonly DbContextApp Db;
        private readonly IHubContext<NotificationsHub> _hubContext;

        public Follow(DbContextApp dbContextApp, IHubContext<NotificationsHub> hubContext)
        {
            Db = dbContextApp;
            _hubContext = hubContext;
        }
        
        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken, [FromForm] int userToFollowId)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var userToFollow = Db.Users.Find(userToFollowId);
            if (userToFollow == null) return NotFound("User to follow was not found");

            // update the following list of user to add the "user to follow"
            var usersUserIsFollowing = JsonSerializer.Deserialize<List<int>>(user.FollowingIdsJson);
            if (!usersUserIsFollowing.Contains(userToFollow.Id))
            {
                usersUserIsFollowing.Add(userToFollow.Id);
                user.FollowingIdsJson = JsonSerializer.Serialize(usersUserIsFollowing);
            }

            user.NumberOfFollowing = usersUserIsFollowing.Count;
            
            // update the followers list of the "user to follow" to add a new follower (the user)
            var followersOfFollowed = JsonSerializer.Deserialize<List<int>>(userToFollow.FollowersIdsJson);
            if (!followersOfFollowed.Contains(user.Id))
            {
                followersOfFollowed.Add(user.Id);
                userToFollow.FollowersIdsJson = JsonSerializer.Serialize(followersOfFollowed);
            }

            userToFollow.NumberOfFollowers = followersOfFollowed.Count;
            
            var notificationsAdministration = new NotificationsAdministration(Db);
            
            var notificationData = notificationsAdministration.CreateNewFollowerNotification(user.Id, userToFollow.Id);
            
            var sessionsId = Hubs.NotificationsHub.Sessions.Where(element => element.Value.Equals(userToFollow.Id));
            
            foreach (var id in sessionsId)
            {
                _hubContext.Clients.Client(id.Key)
                    .SendAsync("fetchNotification",notificationData, NotificationsAdministration.TypeNewFollower);
            }
            
            Db.Users.Update(user);
            Db.Users.Update(userToFollow);
            Db.SaveChanges();
            return Ok("Followers updated!");
        }
        
        [Route("Unfollow")]
        [HttpPost]
        public IActionResult Unfollow([FromForm] string sessionToken, [FromForm] int userToUnfollowId)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var userToUnfollow = Db.Users.Find(userToUnfollowId);
            if (userToUnfollow == null) return NotFound("User to unfollow was not found");
            
            // update the following list of user to remove the "user to unfollow"
            var usersUserIsFollowing = JsonSerializer.Deserialize<List<int>>(user.FollowingIdsJson);
            usersUserIsFollowing.Remove(userToUnfollow.Id);
            user.FollowingIdsJson = JsonSerializer.Serialize(usersUserIsFollowing);
            
            user.NumberOfFollowing = usersUserIsFollowing.Count;
            
            // update the followers list of the "user to follow" to remove the follower (the user)
            var followersOfFollowed = JsonSerializer.Deserialize<List<int>>(userToUnfollow.FollowersIdsJson);
            followersOfFollowed.Remove(user.Id);
            userToUnfollow.FollowersIdsJson = JsonSerializer.Serialize(followersOfFollowed);
            
            userToUnfollow.NumberOfFollowers = followersOfFollowed.Count;
            
            Db.Users.Update(user);
            Db.Users.Update(userToUnfollow);
            Db.SaveChanges();
            return Ok("Followers updated!");
        }

        [Route("Following")]
        [HttpPost]
        public IActionResult Following([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            var ids = JsonSerializer.Deserialize<List<int>>(user.FollowingIdsJson);
            var users = Db.Users.Where(u => ids.Contains(u.Id)).Select(u => new 
            {
                Id = u.Id,
                Name = u.Name,
                ImageUrl = Utils.UrlGenerators.GenerateProfilePictureUrl(u.Id,sessionToken, Request)
            });
            return Ok(users);
        }

        [Route("Followers")]
        [HttpPost]
        public IActionResult Followers([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            var ids = JsonSerializer.Deserialize<List<int>>(user.FollowersIdsJson);
            
            var users = Db.Users.Where(u => ids.Contains(u.Id)).Select(u => new 
            {
                Id = u.Id,
                Name = u.Name,
                ImageUrl = Utils.UrlGenerators.GenerateProfilePictureUrl(u.Id,sessionToken, Request)
            });
            return Ok(users);
        }
    }
}
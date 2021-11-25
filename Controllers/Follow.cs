using System;
using System.Linq;
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

            var followerRelation = new FollowerRelation()
            {
                UserId = user.Id,
                TargetUserId = userToFollow.Id
            };

            Db.FollowerRelations.Add(followerRelation);

            user.NumberOfFollowing = Db.FollowerRelations.Count(relation => relation.UserId.Equals(user.Id));
            userToFollow.NumberOfFollowers =
                Db.FollowerRelations.Count(relation => relation.TargetUserId.Equals(userToFollow.Id));

            Db.Users.Update(user);
            Db.Users.Update(userToFollow);

            var notificationsAdministration = new NotificationsAdministration(Db);

            var notificationData = notificationsAdministration.CreateNewFollowerNotification(user.Id, userToFollow.Id);

            var sessionsId = Hubs.NotificationsHub.Sessions.Where(element => element.Value.Equals(userToFollow.Id));

            foreach (var id in sessionsId)
            {
                _hubContext.Clients.Client(id.Key)
                    .SendAsync("fetchNotification", notificationData, NotificationsAdministration.TypeNewFollower);
            }

            Db.SaveChanges();
            return Ok("Followers updated!");
        }

        [Route("Unfollow")]
        [HttpPost]
        public IActionResult Unfollow([FromForm] string sessionToken, [FromForm] Guid userToUnfollowId)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var userToUnfollow = Db.Users.Find(userToUnfollowId);
            if (userToUnfollow == null) return NotFound("User to unfollow was not found");

            var followerRelation = Db.FollowerRelations.Single(relation =>
                relation.UserId.Equals(user.Id) && relation.TargetUserId.Equals(userToUnfollow.Id));

            Db.FollowerRelations.Remove(followerRelation);

            user.NumberOfFollowing = Db.FollowerRelations.Count(relation => relation.UserId.Equals(user.Id));
            userToUnfollow.NumberOfFollowers =
                Db.FollowerRelations.Count(relation => relation.TargetUserId.Equals(userToUnfollow.Id));

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

            var listOfFollowing = Db.FollowerRelations
                .Where(relation => relation.UserId.Equals(user.Id)).Select(result => new
                {
                    Id = result.TargetUserId,
                    Name = "",
                    ImageUrl = Utils.UrlGenerators.GenerateProfilePictureUrl(result.TargetUserId, sessionToken, Request)
                });
            return Ok(listOfFollowing);
        }

        [Route("Followers")]
        [HttpPost]
        public IActionResult Followers([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var listOfFollowers = Db.FollowerRelations.Where(relation => relation.TargetUserId.Equals(user.Id)).Select(
                result => new
                {
                    Id = result.UserId,
                    Name = "",
                    ImageUrl = Utils.UrlGenerators.GenerateProfilePictureUrl(result.UserId, sessionToken, Request)
                });
            return Ok(listOfFollowers);
        }
    }
}
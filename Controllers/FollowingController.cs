using System;
using System.Linq;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.Hubs;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class FollowingController : Controller
    {
        private readonly DbContextApp Db;
        private readonly IHubContext<NotificationsHub> _hubContext;

        public FollowingController(DbContextApp dbContextApp, IHubContext<NotificationsHub> hubContext)
        {
            Db = dbContextApp;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Route("Follow")]
        public IActionResult Index([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var userToFollow = Db.Users.Find(Convert.ToInt32(identification.Id));
            if (userToFollow == null) return NotFound("User to follow was not found");

            // create new relation only if there is not one already
            if (Db.FollowerRelations.Any(rel => rel.UserId.Equals(user.Id) && rel.TargetUserId.Equals(userToFollow.Id)))
            {
                return Unauthorized("user already followed");
            }

            var followerRelation = new FollowerRelation()
            {
                UserId = user.Id,
                TargetUserId = userToFollow.Id
            };

            Db.FollowerRelations.Add(followerRelation);

            var notificationsAdministration = new NotificationsAdministration(Db);

            var notificationData = notificationsAdministration.CreateNewFollowerNotification(user.Id, userToFollow.Id);

            var sessionsId = Hubs.NotificationsHub.Sessions.Where(element => element.Value.Equals(userToFollow.Id));

            foreach (var id in sessionsId)
            {
                _hubContext.Clients.Client(id.Key)
                    .SendAsync("fetchNotification", notificationData, NotificationsAdministration.TypeNewFollower);
            }

            Db.SaveChanges();

            // update number of followings and followers of involved users
            user.NumberOfFollowing = Db.FollowerRelations.Count(relation => relation.UserId.Equals(user.Id));
            userToFollow.NumberOfFollowers =
                Db.FollowerRelations.Count(relation => relation.TargetUserId.Equals(userToFollow.Id));

            Db.Users.Update(user);
            Db.Users.Update(userToFollow);
            Db.SaveChanges();
            return Ok("Followers updated!");
        }

        [Route("Unfollow")]
        [HttpPost]
        public IActionResult Unfollow([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var userToUnfollow = Db.Users.Find(Convert.ToInt32(identification.Id));
            if (userToUnfollow == null) return NotFound("User to unfollow was not found");

            try
            {
                // finds and removes follower relation
                var followerRelation = Db.FollowerRelations.Single(relation =>
                    relation.UserId.Equals(user.Id) && relation.TargetUserId.Equals(userToUnfollow.Id));
                Db.FollowerRelations.Remove(followerRelation);

                Db.SaveChanges();

                // update number of followings and followers of involved users
                user.NumberOfFollowing = Db.FollowerRelations.Count(relation => relation.UserId.Equals(user.Id));
                userToUnfollow.NumberOfFollowers =
                    Db.FollowerRelations.Count(relation => relation.TargetUserId.Equals(userToUnfollow.Id));
                Db.Users.Update(user);
                Db.Users.Update(userToUnfollow);
                Db.SaveChanges();
                return Ok("Followers updated!");
            }
            catch (InvalidOperationException)
            {
                return Unauthorized("cannot unfollow user, not followed");
            }
        }

        [Route("FollowingsOf/{userId:int}")]
        [HttpGet]
        public IActionResult Following([FromHeader(Name = "sessionToken")] string sessionToken, int userId)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
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
        public IActionResult Followers([FromHeader(Name = "sessionToken")] string sessionToken, int userId)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
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
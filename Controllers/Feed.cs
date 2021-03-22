using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Feed : ControllerBase
    {
        private readonly DbContextApp Db;

        public Feed(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        
        [HttpPost]
        public IActionResult Index([FromForm] int userId,[FromForm] string password,[FromForm] string postsInDom)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User was not found");
            if (!user.Password.Equals(password)) return Unauthorized("Password is not correct");

            var followingIds = JsonSerializer.Deserialize<List<int>>(user.FollowingIdsJson);
            var posts = new List<SimpleTextPost>();
            var renderedPosts = JsonSerializer.Deserialize<List<int>>(postsInDom);
            
            List<UserSeenPostHistory> userPostSeenHistory = Db.UserSeenPostHistories
                .Where(history => history.UserId.Equals(user.Id)).ToList();

            foreach (var followingId in followingIds)
            {
                posts.AddRange(Db.SimpleTextPosts.Where(post => 
                    post.UserId.Equals(followingId) && post.Privacy != 1));
            }

            foreach (var renderedPostId in renderedPosts)
            {
                posts.RemoveAll(post => post.Id.Equals(renderedPostId));
                userPostSeenHistory.RemoveAll(element => element.PostId.Equals(renderedPostId));
            }

            userPostSeenHistory.RemoveAll(element => element.TimesSeen <= 5);

            foreach (var historySeen in userPostSeenHistory)
            {
                posts.RemoveAll(post => post.Id.Equals(historySeen.PostId));
            }

            posts = posts.OrderByDescending(post => post.Id).Take(4).ToList();

            var response = new List<ReturningPostsComposedResponse>();
            foreach (var post in posts)
            {
                response.Add(new ReturningPostsComposedResponse()
                {
                    Id = post.Id,
                    Privacy = post.Privacy,
                    NumberOfLikes = post.NumberOfLikes,
                    TextContent = post.TextContent,
                    UserId = post.UserId,
                    Liked = Db.Likes.Any(element => element.PostId == post.Id && element.UserId == user.Id)
                });
                if (Db.UserSeenPostHistories.Any(element => 
                    element.PostId == post.Id && element.UserId == user.Id))
                {
                    var historyToUpdate = Db.UserSeenPostHistories.Single(element =>
                        element.UserId.Equals(user.Id) && element.PostId == post.Id);
                    historyToUpdate.TimesSeen++;
                    Db.UserSeenPostHistories.Update(historyToUpdate);
                }
                else
                {
                    var historyToAdd = new UserSeenPostHistory()
                    {
                        PostId = post.Id,
                        TimesSeen = 1,
                        UserId = user.Id
                    };
                    Db.UserSeenPostHistories.Add(historyToAdd);
                }
            }
            Db.SaveChanges();
            
            
            return Ok(response);
        }

        [HttpPost]
        [Route("GetUserPosts")]
        public IActionResult GetUserPosts([FromForm] int userId, [FromForm] string password, [FromForm] int accountId)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User was not found");
            if (!user.Password.Equals(password)) return Unauthorized("Password is not correct");
            var account = Db.Users.Find(accountId);
            if (account == null) 
                return NotFound("That account does not exist. Please be sure the accountId parameter is correct");

            var usersLikes = Db.Likes.Where(like => like.UserId == user.Id).ToList();

            var posts = Db.SimpleTextPosts
                .Where(post => post.UserId == account.Id)
                .OrderByDescending(post => post.Id)
                .ToList();
                
            
            var response = posts.Select(post => new ReturningPostsComposedResponse()
                {
                    Id = post.Id,
                    NumberOfLikes = post.NumberOfLikes,
                    Privacy = post.Privacy,
                    TextContent = post.TextContent,
                    UserId = post.UserId,
                    Liked = usersLikes.Any(like => like.PostId == post.Id && like.UserId == user.Id)
                })
                .ToList();
                
            return Ok(response);
        }
    }
}
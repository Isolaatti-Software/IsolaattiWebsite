using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Index([FromForm] string sessionToken, [FromForm] string postsInDom)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var followingIds = JsonSerializer.Deserialize<List<Guid>>(user.FollowingIdsJson);
            var renderedPosts = JsonSerializer.Deserialize<List<Guid>>(postsInDom);

            var posts = (from post in Db.SimpleTextPosts
                where !post.Privacy.Equals(1)
                      && post.UserId.Equals(user.Id)
                select post).ToList();


            var response = new List<ReturningPostsComposedResponse>();
            foreach (var post in posts)
            {
                response.Add(new ReturningPostsComposedResponse(post)
                {
                    UserName = Db.Users.Find(post.UserId).Name,
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
        public IActionResult GetUserPosts([FromForm] string sessionToken, [FromForm] int accountId)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var account = Db.Users.Find(accountId);
            if (account == null)
                return NotFound("That account does not exist. Please be sure the accountId parameter is correct");

            // var usersLikes = Db.Likes.Where(like => like.UserId == user.Id).ToList();

            var posts = Db.SimpleTextPosts
                .Where(post => post.UserId == account.Id && !post.Privacy.Equals(1))
                .OrderByDescending(post => post.Id)
                .ToList();


            var response = posts.Select(post => new ReturningPostsComposedResponse(post)
                {
                    UserName = Db.Users.Find(post.UserId).Name,
                    Liked = Db.Likes.Any(element => element.PostId == post.Id && element.UserId == user.Id)
                })
                .ToList();

            return Ok(response);
        }

        [HttpGet]
        [Route("Public")]
        public IActionResult GetPublicFeed(bool mostLiked = false)
        {
            IEnumerable<ReturningPostsComposedResponse> feed;
            if (mostLiked)
            {
                feed = Db.SimpleTextPosts.Where(post => post.Privacy.Equals(3))
                    .OrderByDescending(post => post.NumberOfLikes)
                    .Take(100).ToList().Select(post => new ReturningPostsComposedResponse(post)
                    {
                        UserName = Db.Users.Find(post.UserId).Name
                    });
            }
            else
            {
                feed = Db.SimpleTextPosts.Where(post => post.Privacy.Equals(3))
                    .OrderByDescending(post => post.Date)
                    .Take(100).ToList().Select(post => new ReturningPostsComposedResponse(post)
                    {
                        UserName = Db.Users.Find(post.UserId).Name
                    });
            }

            return Ok(feed);
        }
    }
}
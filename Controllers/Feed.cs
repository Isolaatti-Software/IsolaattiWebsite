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
        public IActionResult Index([FromForm] string sessionToken,[FromForm] string postsInDom)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var followingIds = JsonSerializer.Deserialize<List<Guid>>(user.FollowingIdsJson);
            // var posts = new List<SimpleTextPost>();
            var renderedPosts = JsonSerializer.Deserialize<List<Guid>>(postsInDom);

            // var userPostSeenHistory = Db.UserSeenPostHistories
            //     .Where(history => history.UserId.Equals(user.Id)).AsEnumerable();

            // these are all the posts from all the people the user follows and the user itself
            // posts.AddRange(Db.SimpleTextPosts
            //     .Where(post =>
            //         post.Privacy != 1 &&
            //         (followingIds.Contains(post.UserId) ||
            //          post.UserId.Equals(user.Id)) &&
            //         !renderedPosts.Contains(post.Id)
            //         && !renderedPosts.Contains(post.Id) &&
            //         userPostSeenHistory.Where(history => history.PostId.Equals(post.Id)));

            
            // foreach (var renderedPostId in renderedPosts)
            // {
            //     userPostSeenHistory.RemoveAll(element => element.PostId.Equals(renderedPostId));
            // }

            var userHistory = Db.UserSeenPostHistories
                .Where(history => history.UserId.Equals(user.Id) && history.TimesSeen <= 10)
                .Select(history => history.PostId).ToList();

            var posts =
                (from post in Db.SimpleTextPosts
                    where !userHistory.Contains(post.Id) && followingIds.Contains(post.UserId) &&
                          !post.Privacy.Equals(1) && !renderedPosts.Contains(post.Id)
                    orderby post.Date descending
                    select post).Take(15).ToList();


            // foreach (var historySeen in userPostSeenHistory)
            // {
            //     posts.RemoveAll(post => post.Id.Equals(historySeen.PostId));
            // }

            // posts = posts.OrderByDescending(post => post.Date).Take(15).ToList();

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
        public IActionResult GetUserPosts([FromForm] string sessionToken, [FromForm] Guid accountId)
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
        public IActionResult GetPublicFeed(bool mostLiked=false)
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

            var response = new List<ComposedResponse>();
            foreach (var post in posts)
            {
                response.Add(new ComposedResponse()
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
    }

    class ComposedResponse
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int UserId { get; set; }
        public long NumberOfLikes { get; set; }
        public int Privacy { get; set; }
        public bool Liked { get; set; }
    }
}
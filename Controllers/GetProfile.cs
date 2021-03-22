/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class GetProfile : ControllerBase
    {
        private readonly DbContextApp Db;

        public GetProfile(DbContextApp _dbContext)
        {
            Db = _dbContext;
        }
        [HttpPost]
        public Profile Index([FromForm]int userId, [FromForm]string password)
        {
            var user = Db.Users.Find(userId);
            if (user.Password.Equals(password))
            {
                var profile = new Profile()
                {
                    Username = user.Name,
                    Email = user.Email,
                    NumberOfSongs = Db.Songs.Count(song => song.OwnerId.Equals(userId)),
                    NumberOfLinks = Db.SharedSongs.Count(sharedLink => sharedLink.userId.Equals(userId))
                };
                return profile;
            }
            return null;
        }

        [HttpPost]
        [Route("GetPosts")]
        public IActionResult GetPosts([FromForm] int userId, [FromForm] string password)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User was not found");
            if (!user.Password.Equals(password)) return Unauthorized("Password is not correct");

            var posts = Db.SimpleTextPosts
                .Where(post => post.UserId == userId);
            posts = posts.OrderByDescending(post => post.Id);

            var likes = Db.Likes.Where(like => like.UserId.Equals(user.Id)).ToList();

            List<ReturningPostsComposedResponse> response = new List<ReturningPostsComposedResponse>();
            foreach (var post in posts)
            {
                response.Add(new ReturningPostsComposedResponse()
                {
                    Id = post.Id,
                    Liked = likes.Any(like => like.PostId.Equals(post.Id)),
                    NumberOfLikes = post.NumberOfLikes,
                    Privacy = post.Privacy,
                    TextContent = post.TextContent,
                    UserId = post.UserId
                });
            }
            
            return Ok(response);
        }
    }
    
}
/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class GetProfile : ControllerBase
    {
        private readonly DbContextApp _db;

        public GetProfile(DbContextApp _dbContext)
        {
            _db = _dbContext;
        }
        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var profile = new Profile()
            {
                Username = user.Name,
                Email = user.Email,
                NumberOfSongs = _db.Songs.Count(song => song.OwnerId.Equals(user.Id)),
                NumberOfLinks = _db.SharedSongs.Count(sharedLink => sharedLink.userId.Equals(user.Id))
            };
            return Ok(profile);
        }

        [HttpPost]
        [Route("GetPosts")]
        public IActionResult GetPosts([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var posts = _db.SimpleTextPosts
                .Where(post => post.UserId == user.Id);
            posts = posts.OrderByDescending(post => post.Id);

            var likes = _db.Likes.Where(like => like.UserId.Equals(user.Id)).ToList();
            var comments = _db.Comments.Where(comment => comment.TargetUser.Equals(user.Id)).ToList();
            
            List<ReturningPostsComposedResponse> response = new List<ReturningPostsComposedResponse>();
            foreach (var post in posts)
            {
                response.Add(new ReturningPostsComposedResponse()
                {
                    Id = post.Id,
                    Liked = likes.Any(like => like.PostId.Equals(post.Id)),
                    NumberOfLikes = post.NumberOfLikes,
                    NumberOfComments = comments.Count(comment => comment.SimpleTextPostId.Equals(post.Id)),
                    Privacy = post.Privacy,
                    TextContent = post.TextContent,
                    UserId = post.UserId,
                    UserName = user.Name
                });
            }
            
            return Ok(response);
        }
    }
    
}
/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System;
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
                Email = user.Email
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
                .Where(post => post.UserId == user.Id)
                .OrderByDescending(post => post.Id).ToList();
            
            List<ReturningPostsComposedResponse> response = new List<ReturningPostsComposedResponse>();
            foreach (var post in posts.ToList())
            {
                response.Add(new ReturningPostsComposedResponse(post)
                {
                    UserName = _db.Users.Find(post.UserId).Name,
                    Liked = _db.Likes.Any(element => element.PostId == post.Id && element.UserId == user.Id)
                });
            }
            
            return Ok(response);
        }

        [HttpPost]
        [Route("Public/{id}")]
        public IActionResult GetPublicProfile([FromForm] string sessionToken, Guid id)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var askedAccount = _db.Users.Find(id);
            if (askedAccount == null) return NotFound("User not found");
            
            
            return Ok(new
            {
                Id = askedAccount.Id,
                Name = askedAccount.Name,
                Description = askedAccount.DescriptionText,
                AudioDescriptionUrl = askedAccount.DescriptionAudioUrl,
                ProfileImageUrl = Utils.UrlGenerators.GenerateProfilePictureUrl(askedAccount.Id,sessionToken,Request)
            });
        }
        
    }
    
}
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    /*
     * This Controller receives a parameter called "privacy". It is an integer that means:
     * 1: Private
     * 2: Only available for Isolaatti users
     * 3: Available for everyone
     */
    
    [ApiController]
    [Route("/api/[controller]")]
    public class MakePost : ControllerBase
    {
        private readonly DbContextApp Db;
        
        public MakePost(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        
        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken, [FromForm] string password, 
            [FromForm] int privacy = 1, 
            [FromForm] string content = "Well, this post was made without content. Why? Idk",
            [FromForm] string audioUrl = null)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            // Yep, here I can create the post
            var newPost = new SimpleTextPost()
            {
                UserId = user.Id,
                TextContent = content,
                Privacy = privacy,
                AudioAttachedUrl = audioUrl
            };

            Db.SimpleTextPosts.Add(newPost);
            Db.SaveChanges();
            
            return Ok(new ReturningPostsComposedResponse(newPost)
            {
                UserName = Db.Users.Find(newPost.UserId).Name,
                Liked = Db.Likes.Any(element => element.PostId == newPost.Id && element.UserId == user.Id)
            });
        }

        [HttpPost]
        [Route("WithProject")]
        public IActionResult PostProject([FromForm] int userId, [FromForm] string password, 
            [FromForm] int privacy, [FromForm] string content, [FromForm] int projectId)
        {
            return Ok();
        }
    }
}
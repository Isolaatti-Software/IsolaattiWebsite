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
        public IActionResult Index([FromForm] int userId, [FromForm] string password, 
            [FromForm] int privacy = 1, 
            [FromForm] string content = "Well, this post was made without content. Why? Idk")
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User does not exist");
            if (!user.Password.Equals(password)) return Unauthorized("Password is not correct");
            // Yep, here I can create the post
            var newPost = new SimpleTextPost()
            {
                UserId = user.Id,
                TextContent = content,
                Privacy = privacy
            };

            Db.SimpleTextPosts.Add(newPost);
            Db.SaveChanges();
            
            return Ok(newPost);
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
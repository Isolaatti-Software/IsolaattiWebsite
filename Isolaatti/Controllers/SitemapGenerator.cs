using Isolaatti.Models;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    public class SitemapGenerator : ControllerBase
    {
        private readonly DbContextApp _db;

        public SitemapGenerator(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        // [HttpGet]
        // [Route("/sitemap.txt")]
        // public IActionResult Index()
        // {
        //     var response = "https://isolaatti.com/MakeAccount\n";
        //     response += "https://isolaatti.com/LogIn\n";
        //     response += "https://isolaatti.com/s/Welcome\n";
        //     response += "https://isolaatti.com/s/Features\n";
        //     response += "https://isolaatti.com/About\n";
        //     var publicPosts = _db.SimpleTextPosts.Where(post => post.Privacy.Equals(3)).ToList();
        //     foreach (var post in publicPosts)
        //     {
        //         response += $"https://isolaatti.com/PublicContent/PublicThreadViewer?id={post.Id}\n";
        //     }
        //     return Ok(response);
        // }
    }
}
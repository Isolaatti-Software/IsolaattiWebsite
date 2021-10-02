using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.PublicContent
{
    public class Feed : PageModel
    {
        private readonly DbContextApp _db;
        public List<SimpleTextPost> FeedContent;
        public Feed(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        public IActionResult OnGet()
        {
            FeedContent = _db.SimpleTextPosts.Where(post => post.Privacy.Equals(3)).ToList();
            return Page();
        }
    }
}
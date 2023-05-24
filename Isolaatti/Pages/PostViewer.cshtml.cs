using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages
{
    [IsolaattiAuth]
    public class Threads : IsolaattiPageModel
    {
        private readonly DbContextApp _db;
        public long PostId;
        
        public Threads(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        public async Task<IActionResult> OnGet([FromRoute] long id)
        {
            var post = await _db.SimpleTextPosts.FindAsync(id);
            if (post == null) return NotFound();
            PostId = post.Id;
            
            return Page();
        }
    }
}
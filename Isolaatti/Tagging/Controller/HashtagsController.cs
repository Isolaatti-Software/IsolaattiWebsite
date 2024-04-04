using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.Models;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Tagging.Controller;

[ApiController]
[Route("/api/hashtags")]
public class HashtagsController : IsolaattiController
{

    private DbContextApp _db;
    public HashtagsController(DbContextApp db)
    {
        _db = db;
    }

    
    [IsolaattiAuth]
    [HttpGet]
    [Route("hashtag/{hashtag}")]
    public async Task<IActionResult> GetHashtagPosts(string hashtag, [FromQuery] long after = -1, [FromQuery] int length = 20)
    {
        var total = _db.Hashtags.Count(ht => ht.Text.Equals(hashtag));
        
        var posts = from ht in _db.Hashtags
            from post in _db.SimpleTextPosts
            where ht.Text.Equals(hashtag) && post.Id == ht.PostId && post.Id > after
            select post;

        return Ok(new ContentListWrapper<Post>()
        {
            Data = await posts.Take(20).ToListAsync(),
            MoreContent = total > length
        });
    }
    
    
    [IsolaattiAuth]
    [HttpGet]
    [Route("trending")]
    public async Task<IActionResult> GetTrendingHashtags()
    {
        var query = await  _db.Hashtags
            .OrderByDescending(ht => ht.Id)
            .GroupBy(ht => ht.Text)
            .OrderByDescending(ht => ht.Count())
            .Take(50)
            .Select(g => g.Key).ToListAsync();
        return Ok(new
        {
            result = query
        });
    }
}
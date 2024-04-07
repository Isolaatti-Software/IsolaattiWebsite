using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.DTOs;
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
    public async Task<IActionResult> GetHashtagPosts(string hashtag, [FromQuery] long after = 0, [FromQuery] int length = 20)
    {
        var total = _db.Hashtags.Count(ht => ht.Text.Equals(hashtag));

        IQueryable<Post> postsQuery;
        
        

        if (after <= 1)
        {
            postsQuery = from ht in _db.Hashtags
                from post in _db.SimpleTextPosts
                where ht.Text.Equals(hashtag) && post.Id == ht.PostId
                orderby post.Id descending
                select post;
        }
        else
        {
            postsQuery = from ht in _db.Hashtags
                from post in _db.SimpleTextPosts
                where ht.Text.Equals(hashtag) && post.Id == ht.PostId && post.Id < after
                orderby post.Id descending 
                select post;
        }

        var posts = postsQuery.Select(post => new PostDto()
        {
            Post = post,
            UserName = _db.Users.FirstOrDefault(u => u.Id == post.UserId).Name,
            NumberOfComments = post.Comments.Count,
            NumberOfLikes = post.Likes.Count,
            Liked = _db.Likes.Any(l => l.UserId == User.Id && l.PostId == post.Id),
            SquadName = post.Squad.Name
        });
        

        return Ok(new ContentListWrapper<PostDto>()
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
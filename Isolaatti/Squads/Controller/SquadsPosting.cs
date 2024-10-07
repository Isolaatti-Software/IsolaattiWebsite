using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Posting.Data;
using Isolaatti.Posting.Dto;
using Isolaatti.Squads.Repository;
using Isolaatti.Utils;

using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Squads.Controller;

[ApiController]
[Route("/api/Squads/{squadId:guid}/Posting")]
public class SquadsPosting : ProfileRequiredController
{
    private readonly DbContextApp _db;
    private readonly ProfileService _accounts;
    private readonly SquadsRepository _squads;

    public SquadsPosting(DbContextApp dbContextApp, ProfileService accounts, SquadsRepository squadsRepository)
    {
        _db = dbContextApp;
        _accounts = accounts;
        _squads = squadsRepository;
    }
    
    
    [HttpGet]
    [Route("Posts")]
    public async Task<IActionResult> GetPostsOfSquad(Guid squadId, long lastId, int length, bool olderFirst)
    {
        var squad = await _squads.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new
            {
                error = "Squad not found"
            });
        }

        if (!await _squads.UserBelongsToSquad(Profile.Id, squad.Id))
        {
            return Unauthorized(new
            {
                error = "User is not authorized to get posts from this squad"
            });
        }
        
        IQueryable<Post> posts;
        
        if (lastId < 0)
        {
            posts = _db.SimpleTextPosts
                .Where(post => post.SquadId.Equals(squad.Id))
                .OrderByDescending(post => post.Id);
        }
        else
        {
            posts = _db.SimpleTextPosts
                .Where(post => post.SquadId.Equals(squad.Id) && post.Id < lastId)
                .OrderByDescending(post => post.Id);
        }

        var total = posts.Count();
        posts = posts.Take(length);
        var feed = from post in posts
            select new PostDto
            {
                Post = post,
                UserName = _db.Users.FirstOrDefault(u => u.Id == post.UserId).Name,
                NumberOfComments = post.Comments.Count,
                NumberOfLikes = post.Likes.Count,
                Liked = _db.Likes.Any(l => l.UserId == Profile.Id && l.PostId == post.Id),
                SquadName = post.Squad.Name
            };
        
        
        return Ok(new ContentListWrapper<PostDto>
        {
            Data = feed.ToList(),
            MoreContent = total > length
        });
    }
}
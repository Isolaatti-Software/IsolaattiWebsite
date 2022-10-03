using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Squads/{squadId:guid}/Posting")]
public class SquadsPosting : ControllerBase
{
    private readonly DbContextApp _db;
    private readonly IAccounts _accounts;
    private readonly SquadsRepository _squads;

    public SquadsPosting(DbContextApp dbContextApp, IAccounts accounts, SquadsRepository squadsRepository)
    {
        _db = dbContextApp;
        _accounts = accounts;
        _squads = squadsRepository;
    }
    
    [HttpGet]
    [Route("Posts")]
    public async Task<IActionResult> GetPostsOfSquad([FromHeader(Name = "sessionToken")] string sessionToken, 
        Guid squadId, long lastId, int length, bool olderFirst)
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");

        var squad = await _squads.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new
            {
                error = "Squad not found"
            });
        }

        if (!await _squads.UserBelongsToSquad(user.Id, squad.Id))
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
        posts = from post in posts
            select post
                .SetLiked(_db.Likes.Any(l => l.PostId == post.Id && l.UserId == user.Id))
                .SetNumberOfComments(_db.Comments.Count(c => c.SimpleTextPostId == post.Id))
                .SetNumberOfLikes(_db.Likes.Count(l => l.PostId == post.Id))
                .SetSquadName(_db.Squads.FirstOrDefault(s => s.Id.Equals(post.SquadId)).Name)
                .SetUserName(_accounts.GetUsernameFromId(post.UserId));
        var feed = posts.ToList();
        
        
        return Ok(new ContentListWrapper<Post>
        {
            Data = feed,
            MoreContent = total > length
        });
    }
}
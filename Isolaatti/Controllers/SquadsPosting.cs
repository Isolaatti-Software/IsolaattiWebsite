using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts;
using Isolaatti.Accounts.Service;
using Isolaatti.Classes;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Squads/{squadId:guid}/Posting")]
public class SquadsPosting : IsolaattiController
{
    private readonly DbContextApp _db;
    private readonly IAccountsService _accounts;
    private readonly SquadsRepository _squads;

    public SquadsPosting(DbContextApp dbContextApp, IAccountsService accounts, SquadsRepository squadsRepository)
    {
        _db = dbContextApp;
        _accounts = accounts;
        _squads = squadsRepository;
    }
    
    [IsolaattiAuth]
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

        if (!await _squads.UserBelongsToSquad(User.Id, squad.Id))
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
                Liked = _db.Likes.Any(l => l.UserId == User.Id && l.PostId == post.Id),
                SquadName = post.Squad.Name
            };
        
        
        return Ok(new ContentListWrapper<PostDto>
        {
            Data = feed.ToList(),
            MoreContent = total > length
        });
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using Isolaatti.Accounts.Data;
using Isolaatti.Accounts.Service;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Posting.Data;
using Isolaatti.Posting.Dto;
using Isolaatti.Squads.Repository;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Isolaatti.Squads.Controller;

[ApiController]
[Route("/api/Squads/{squadId:guid}/Posting")]
public class SquadsPosting : IsolaattiController
{
    private readonly DbContextApp _db;
    private readonly IAccountsService _accounts;
    private readonly SquadsRepository _squads;
    private readonly ILogger<SquadsPosting> _logger;

    public SquadsPosting(DbContextApp dbContextApp, IAccountsService accounts, SquadsRepository squadsRepository, ILogger<SquadsPosting> logger)
    {
        _db = dbContextApp;
        _accounts = accounts;
        _squads = squadsRepository;
        _logger = logger;
    }
    
    [Authorize]
    [HttpGet]
    [Route("Posts")]
    public async Task<IActionResult> GetPostsOfSquad(Guid squadId, long lastId, int length, bool olderFirst)
    {
        var squad = await _squads.GetSquad(squadId, User.Id);
        if (squad == null)
        {
            return NotFound(new
            {
                error = "Squad not found"
            });
        }

        if (!await _squads.UserBelongsToSquad(squad.Id, User.Id))
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
                .Include(post => post.Images)
                .Where(post => post.SquadId.Equals(squad.Id))
                .OrderByDescending(post => post.Id);
        }
        else
        {
            posts = _db.SimpleTextPosts
                .Include(post => post.Images)
                .Where(post => post.SquadId.Equals(squad.Id) && post.Id < lastId)
                .OrderByDescending(post => post.Id);
        }

        var total = posts.Count();
        posts = posts.Take(length);
        var feed = from post in posts
            select new PostDto
            {
                Post = post,
                UserName = post.User.Name,
                NumberOfComments = _db.Comments.Count(comment => comment.PostId == post.Id),
                NumberOfLikes = _db.Likes.Count(like => like.PostId == post.Id),
                Liked = _db.Likes.Any(like => like.PostId == post.Id && like.UserId == User.Id),
                Participants = post.Comments
                    .GroupBy(comment => comment.UserId)
                    .Select(comment => new UserFeedDto()
                {
                    Id = comment.First().UserId,
                    Name = comment.First().User.Name,
                    Following =  false,
                }).ToList(),
                SquadName = post.Squad.Name
            };
        
        
        return Ok(new ContentListWrapper<PostDto>
        {
            Data = feed.ToList(),
            MoreContent = total > length
        });
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
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
        
        IQueryable<SimpleTextPost> posts;
        var likes = _db.Likes.Where(like => like.UserId == user.Id);
        
        if (lastId < 0)
        {
            posts = _db.SimpleTextPosts
                .Where(post => post.SquadId.Equals(squad.Id))
                .OrderByDescending(post => post.Id).Take(length);
        }
        else
        {
            posts = _db.SimpleTextPosts
                .Where(post => post.SquadId.Equals(squad.Id) && post.Id < lastId)
                .OrderByDescending(post => post.Id).Take(length);
        }
        
        var feed = posts
            .Select(post => new
            {
                postData = new FeedPost
                {
                    Id = post.Id,
                    Username = post.User.Name,
                    UserId = post.UserId,
                    Liked = likes.Any(element => element.PostId == post.Id && element.UserId == user.Id),
                    Content = post.TextContent,
                    NumberOfLikes = post.NumberOfLikes,
                    NumberOfComments = post.NumberOfComments,
                    Privacy = post.Privacy,
                    AudioId = post.AudioId,
                    TimeStamp = post.Date,
                    UserIsOwner = post.UserId == user.Id,
                    SquadId = post.SquadId,
                    SquadName = post.Squad.Name
                    // the other attributes are null, but they can be useful in the future
                }
            }).ToList();
        
        long lastPostId;
        try
        {
            lastPostId = feed.Last().postData.Id;
        }
        catch (InvalidOperationException)
        {
            lastPostId = -1;
        }
        
        return Ok(new
        {
            squadName = squad.Name,
            feed,
            moreContent = feed.Count == length,
            lastId = lastPostId
        });
    }
}
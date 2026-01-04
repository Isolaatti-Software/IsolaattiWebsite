using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Database.EF;
using Isolaatti.Images.Repository;
using Isolaatti.Posting.Dto;
using Isolaatti.Search.Dto;
using Isolaatti.Squads.Repository;
using Isolaatti.Users.Repository;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Search.Controller;

[ApiController]
[Route("/api/Search")]
public class SearchController : IsolaattiController
{
    private readonly DbContextApp _db;
    private readonly ImagesRepository _imagesRepository;
    private readonly SquadUsersRepository _squadUsersRepository;
    private readonly SquadsRepository _squadsRepository;
    private readonly UsersRepository _usersRepository;

    public SearchController(DbContextApp db, ImagesRepository imagesRepository, SquadUsersRepository squadUsersRepository, SquadsRepository squadsRepository, UsersRepository usersRepository)
    {
        _db = db;
        _imagesRepository = imagesRepository;
        _squadUsersRepository = squadUsersRepository;
        _squadsRepository = squadsRepository;
        _usersRepository = usersRepository;
    }

    [Authorize]
    [Route("Quick")]
    [HttpGet]
    public async Task<IActionResult> QuickSearch([FromQuery] string q, [FromQuery] string contextType = "global", 
        string? contextValue = null, [FromQuery] bool onlyProfile = false)
    {
        var results = new SearchResult();
        if (q.IsNullOrWhiteSpace())
        {
            return Ok(results);
        }

        var lowerCaseQ = q.ToLower();


        // Perform basic search on profiles
        var profilesResults = from u in _db.Users
            where u.Name.ToLower().Contains(lowerCaseQ)
            orderby u.Id
            select new UserFeedDto
            {
                Id = u.Id,
                ImageId = u.ProfileImageId,
                Name = u.Name
            };


        // perform another filtering so that users already in the squad don't get listed as search result
        if (contextType.Equals("squad") && contextValue != null)
        {
            try
            {
                var squadId = Guid.Parse(contextValue);
                if (_db.SquadUsers.Any(squadUser => squadUser.SquadId.Equals(squadId)))
                {
                    profilesResults = from profileResult in profilesResults
                        from squadUser in _db.SquadUsers
                        where squadUser.SquadId.Equals(squadId) && !squadUser.UserId.Equals(profileResult.Id)
                        orderby profileResult.Id
                        select profileResult;
                }
            }
            catch (FormatException)
            {
            }
        }

        results.Profiles.AddRange(await profilesResults.Take(10).ToListAsync());

        if (onlyProfile)
        {
            return Ok(results);
        }

        // Perform basic search on posts
        var postsResults = from post in _db.SimpleTextPosts
            where post.TextContent.ToLower().Contains(lowerCaseQ) && post.SquadId == null
            orderby post.Id
            select new PostDto
            {
                Post = post,
                Liked = _db.Likes.Any(l => l.UserId == User.Id && l.PostId == post.Id),
                NumberOfComments = _db.Comments.Count(c => c.PostId == post.Id),
                NumberOfLikes = _db.Likes.Count(l => l.PostId == post.Id),
                SquadName = _db.Squads.Where(s => s.Id.Equals(post.SquadId)).Select(s => s.Name).FirstOrDefault(),
                UserName = _db.Users.Where(u => u.Id == post.UserId).Select(u => u.Name).FirstOrDefault()
            };

        results.Posts.AddRange(postsResults.Take(10).ToList());

        // Perform basic search on squads
        var squadsResults = from squad in _db.Squads
            where squad.Name.ToLower().Contains(lowerCaseQ) || squad.Description.ToLower().Contains(lowerCaseQ) ||
                  squad.ExtendedDescription.ToLower().Contains(lowerCaseQ)
            select squad;

        results.Squads.AddRange(await squadsResults.Take(8).ToListAsync());
        
        // Some post processing
        // Add to users result the authors from the posts, audios and images
        var postsAuthors = 
            from po in results.Posts.DistinctBy(po => po.Post.Id) select po.Post.UserId;

        var absentUsers = 
            from postAuthor in postsAuthors.Except(results.Profiles.Select(p => p.Id)) select postAuthor;

        foreach (var absentUserId in absentUsers)
        {
            var absentUser = _db.Users.Where(u => u.Id == absentUserId).Select(u => new UserFeedDto()
            {
                Id = u.Id,
                Name = u.Name,
                ImageId = u.ProfileImageId
            }).FirstOrDefault();
            if (absentUsers != null)
            {
                results.Profiles.Add(absentUser);
            }
        }
        return Ok(results);
    }

    [Authorize]
    [HttpGet]
    [Route("v2")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        var result = await _db.SearchView
            .FromSql(
                $"SELECT origin_table, id, title, description FROM View_SearchItems WHERE  View_SearchItems.searchable_element @@ to_tsquery('english', {query.Replace(" ", "|")} )")
            .ToListAsync();

        return Ok(new { result });
    }

    [Authorize]
    [HttpGet]
    [Route("newestUsers")]
    public async Task<IActionResult> GetNewestUsers([FromQuery] int limit, [FromQuery] int after)
    {
        var users = (from user in _db.Users
            where user.Id != User.Id
            orderby user.Id descending
            select new UserFeedDto()
            {
                Id = user.Id,
                Name = user.Name,
                ImageId = user.ProfileImageId
            })
            .ToList()
            .Select(searchResult => { 
                searchResult.Following = _usersRepository.GetUserFollowsUser(User.Id, searchResult.Id); 
                return searchResult; 
            });

        return Ok(new
        {
            result = users.ToList()
        });
    }
}
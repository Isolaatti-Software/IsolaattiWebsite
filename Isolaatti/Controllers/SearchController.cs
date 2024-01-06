using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.DTOs;
using Isolaatti.Helpers;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Search")]
public class SearchController : IsolaattiController
{
    private readonly DbContextApp _db;
    private readonly AudiosRepository _audios;
    private readonly ImagesRepository _imagesRepository;
    private readonly SquadUsersRepository _squadUsersRepository;
    private readonly SquadsRepository _squadsRepository;

    public SearchController(DbContextApp db, AudiosRepository audios, ImagesRepository imagesRepository, SquadUsersRepository squadUsersRepository, SquadsRepository squadsRepository)
    {
        _db = db;
        _audios = audios;
        _imagesRepository = imagesRepository;
        _squadUsersRepository = squadUsersRepository;
        _squadsRepository = squadsRepository;
    }

    [IsolaattiAuth]
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

        // Perform basic search on audios
        var audiosResult = await _audios.SearchByName(lowerCaseQ);

        results.Audios.AddRange(audiosResult.Select(a => new FeedAudio(a)
        {
            UserName = _db.Users.Where(u => u.Id == a.UserId).Select(u => u.Name).FirstOrDefault()
        }));

        // Perform basic search on images
        var imagesResult = await _imagesRepository.SearchOnName(lowerCaseQ);
        results.Images.AddRange(imagesResult.Select(i => new ImageFeed(i)
        {
            Username = _db.Users.Where(u => u.Id == i.UserId).Select(u => u.Name).FirstOrDefault()
        }));
        
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

    [IsolaattiAuth]
    [Route("{squadId:guid}/RankedUserSearch")]
    [HttpGet]
    public async Task<IActionResult> RankedSearchOnSquads([FromQuery] string q, Guid squadId, [FromQuery] int lastId = 0)
    {
        if (q.IsNullOrWhiteSpace())
        {
            return NoContent();
        }

        var squadExists = await _squadsRepository.SquadExists(squadId);

        var userBelongs = await _squadsRepository.UserBelongsToSquad(User.Id, squadId);

        if (!squadExists || !userBelongs)
        {
            return NotFound();
        }
        
        var result = await _squadUsersRepository.SearchOnUsers(q, squadId, lastId);
        return Ok(result);
    }

    [IsolaattiAuth]
    [Route("{squadId:guid}/SearchSuggestions")]
    [HttpGet]
    public async Task<IActionResult> GetSuggestionsSquadRankedUsers(Guid squadId,[FromQuery] bool owner = false, [FromQuery] bool admins = true, [FromQuery] bool members = true)
    {
        var squadExists = await _squadsRepository.SquadExists(squadId);

        var userBelongs = await _squadsRepository.UserBelongsToSquad(User.Id, squadId);

        if (!squadExists || !userBelongs)
        {
            return NotFound();
        }

        var result = await _squadUsersRepository.GetRankedSuggestions(squadId, owner, admins, members);
        return Ok(result);
    }
}
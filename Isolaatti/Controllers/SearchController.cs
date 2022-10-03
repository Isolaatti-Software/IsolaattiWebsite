using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Enums;
using Isolaatti.Helpers;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Search")]
public class SearchController : ControllerBase
{
    private readonly DbContextApp _db;
    private readonly IAccounts _accounts;
    private readonly AudiosRepository _audios;
    private readonly SquadInvitationsRepository _squadInvitationsRepository;
    private readonly SquadsRepository _squadsRepository;

    public SearchController(DbContextApp db, 
        IAccounts accounts, 
        AudiosRepository audios, 
        SquadInvitationsRepository squadInvitationsRepository, 
        SquadsRepository squadRepository)
    {
        _db = db;
        _audios = audios;
        _accounts = accounts;
        _squadInvitationsRepository = squadInvitationsRepository;
        _squadsRepository = squadRepository;
    }

    [Route("Quick")]
    [HttpGet]
    public async Task<IActionResult> QuickSearch([FromHeader(Name = "sessionToken")] string sessionToken,
        [FromQuery] string q, [FromQuery] string contextType = "global", string contextValue = null, [FromQuery] bool onlyProfile = false )
    {
        var user = await _accounts.ValidateToken(sessionToken);
        if (user == null) return Unauthorized("Token is not valid");


        var results = new List<SearchResult>();
        if (q.IsNullOrWhiteSpace())
        {
            return Ok(results);
        }

        var lowerCaseQ = q.ToLower();
        

        // Perform basic search on profiles
        var profilesResults = from u in _db.Users
            where u.Name.ToLower().Contains(lowerCaseQ)
            select new SearchResult
            {
                ResultType = SearchResultType.Profile,
                ResourceId = u.Id,
                ContentPreview = u.Name
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
                        where squadUser.SquadId.Equals(squadId) && !squadUser.UserId.Equals(profileResult.ResourceId)
                        select profileResult;
                }
                
                
            }
            catch (FormatException)
            {
                
            }

        }
        results.AddRange(profilesResults.Take(20));
        
        if (onlyProfile)
        {
            return Ok(results);
        }

        // Perform basic search on posts
        var postsResults = from post in _db.SimpleTextPosts
            where post.TextContent.ToLower().Contains(lowerCaseQ)
            select new SearchResult
            {
                ResultType = SearchResultType.Post,
                ResourceId = post.Id.ToString(),
                ContentPreview = post.TextContent.Substring(0, 30)
            };

        results.AddRange(postsResults.Take(20));


        return Ok(results);
    }
}
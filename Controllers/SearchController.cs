using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;
using NUglify.Helpers;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Search")]
public class SearchController : ControllerBase
{
    private readonly DbContextApp _db;
    private readonly IAccounts _accounts;
    private readonly AudiosRepository _audios;

    public SearchController(DbContextApp db, IAccounts accounts, AudiosRepository audios)
    {
        _db = db;
        _audios = audios;
        _accounts = accounts;
    }

    [Route("Quick")]
    [HttpGet]
    public async Task<IActionResult> QuickSearch([FromHeader(Name = "sessionToken")] string sessionToken,
        [FromQuery] string q)
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
                ResourceId = u.Id.ToString(),
                ContentPreview = u.Name
            };

        results.AddRange(profilesResults);

        // Perform basic search on posts
        var postsResults = from post in _db.SimpleTextPosts
            where post.TextContent.ToLower().Contains(lowerCaseQ)
            select new SearchResult
            {
                ResultType = SearchResultType.Post,
                ResourceId = post.Id.ToString(),
                ContentPreview = post.TextContent.Substring(0, 30)
            };

        results.AddRange(postsResults);


        return Ok(results);
    }
}
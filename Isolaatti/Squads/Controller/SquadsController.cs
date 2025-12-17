using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Database.EF;
using Isolaatti.EmailSender;
using Isolaatti.Enums;
using Isolaatti.Squads.Data;
using Isolaatti.Squads.Dto;
using Isolaatti.Squads.Repository;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Squads.Controller;

[ApiController]
[Route("/api/Squads")]
public class SquadsController : IsolaattiController
{
    private readonly SquadsRepository _squadsRepository;
    private readonly DbContextApp _db;
    private readonly EmailSenderMessaging _emailSenderMessaging;

    public SquadsController(
        SquadsRepository squadsRepository, 
        DbContextApp dbContextApp, 
        EmailSenderMessaging emailSenderMessaging)
    {
        _squadsRepository = squadsRepository;
        _db = dbContextApp;
        _emailSenderMessaging = emailSenderMessaging;
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> MakeSquad(SquadCreationRequest payload)
    {
        var creationResult = await _squadsRepository.MakeSquad(User.Id, payload);
        if (creationResult.CreationResult == SquadCreationResult.Success)
        {
            return Ok(creationResult);
        }
        return BadRequest(creationResult);
        
    }
    
    [IsolaattiAuth]
    [HttpGet]
    [Route("{squadId:guid}")]
    public async Task<IActionResult> GetSquad(Guid squadId)
    {
        var squad = await _squadsRepository.GetSquad(squadId, User.Id);
        if (squad == null)
        {
            return BadRequest("Squad not found");
        }

        if (squad.ImageId != null)
        {
            squad.ImageUrl = Url.Link("GetImage", new { imageId = squad.ImageId });
        }
        
        return Ok(squad);
    }
    
    [IsolaattiAuth]
    [HttpDelete]
    [Route("{squadId:guid}")]
    public async Task<IActionResult> RemoveSquad(Guid squadId)
    {
        
        var deleted = await _squadsRepository.RemoveSquad(squadId, User.Id);

        if (!deleted)
        {
            return NotFound();
        }
        
        return Ok(new { deleted });
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetSquads(Guid? lastId, [FromQuery] int pageSize = 20)
    {
        var squads = await _squadsRepository.GetSquads(User.Id, lastId);

        foreach (var squadListItemDto in squads)
        {
            if (squadListItemDto.ImageId != null)
            {
                squadListItemDto.ImageUrl = Url.Link("GetImage", new { imageId = squadListItemDto.ImageId });
            }
        }

        var hasMore = squads.Count > pageSize;

        if (hasMore)
        {
            squads.RemoveAt(squads.Count - 1);
        }
        
        return Ok(new
        {
            squads,
            hasMore,
            nextCursor = squads.LastOrDefault()?.Id
        });
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("Discover")]
    public async Task<IActionResult> DiscoverSquads(Guid? lastId, [FromQuery] int pageSize = 20)
    {
        var squads = await _squadsRepository.GetSquadsDiscover(User.Id, lastId);

        foreach (var squadListItemDto in squads)
        {
            if (squadListItemDto.ImageId != null)
            {
                squadListItemDto.ImageUrl = Url.Link("GetImage", new { imageId = squadListItemDto.ImageId });
            }
        }

        var hasMore = squads.Count > pageSize;

        if (hasMore)
        {
            squads.RemoveAt(squads.Count - 1);
        }
        
        return Ok(new
        {
            squads,
            hasMore,
            nextCursor = squads.LastOrDefault()?.Id
        });
    }

    [IsolaattiAuth]
    [HttpPut]
    [Route("{squadId:guid}")]
    public async Task<IActionResult> UpdateSquad(Guid squadId, SquadUpdateInfoRequest payload)
    {
        var result = await _squadsRepository.UpdateSquad(squadId, User.Id, payload.Name, payload.Description,
            payload.ExtendedDescription);

        if (result == SquadUpdateResult.ValidationErrors)
        {
            return BadRequest(new
            {
                result
            });
        }

        if (result == SquadUpdateResult.Error)
        {
            return Problem();
        }

        if (result == SquadUpdateResult.SquadDoesNotExist)
        {
            return NotFound(new
            {
                result
            });
        }
        return Ok(new
        {
            result
        });
    }
    

    [IsolaattiAuth]
    [HttpGet]
    [Route("Feed")]
    public async Task<IActionResult> GetFeed(long lastPostId)
    {
        var posts = from post in _db.SimpleTextPosts
            join squadUser in _db.SquadUsers on post.SquadId equals squadUser.SquadId into squadUsers
            from squadUser in squadUsers.DefaultIfEmpty()
            where squadUser.UserId.Equals(User.Id)
            select post;
        return Ok(new
        {
            posts = await posts.ToListAsync()
        });
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Database.EF;
using Isolaatti.EmailSender;
using Isolaatti.Enums;
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
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return BadRequest("Squad not found");
        }
        squad.OwnedByUser = squad.UserId == User.Id;
        return Ok(new
        {
            squad = squad,
            state = new SquadStateDto()
            {
                IsOwner = squad.UserId == User.Id
            }
        });
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("{squadId:guid}/MyState")]
    public async Task<IActionResult> GetSquadState(Guid squadId)
    {
        return Ok(await _squadsRepository.GetSquadState(squadId, User.Id));
    }
    
    [IsolaattiAuth]
    [HttpDelete]
    [Route("{squadId:guid}")]
    public async Task<IActionResult> RemoveSquad(Guid squadId)
    {
        if (!await _squadsRepository.ValidateSquadOwner(squadId, User.Id))
        {
            return Unauthorized(new { error = "This squad cannot by deleted by this user." });
        }
        
        await _squadsRepository.RemoveSquad(squadId);
        
        return Ok();
    }
    
    

    [IsolaattiAuth]
    [HttpGet]
    [Route("MySquads")]
    public async Task<IActionResult> GetSquadOfUserAdmins(Guid? lastId)
    {
        return Ok(await _squadsRepository.GetSquadsUserOwns(User.Id, lastId).ToListAsync());
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("SquadsBelong")]
    public async Task<IActionResult> GetSquadsOfUser(Guid? lastId)
    {
        return Ok(_squadsRepository.GetSquadUserBelongs(User.Id, lastId));
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("")]
    public IActionResult GetSquads(Guid? lastId)
    {
        return Ok(new
        {
            squads = _squadsRepository.GetSquads(User.Id, lastId).ToList()
                .Select(s => s.Apply(squad => squad.OwnedByUser = squad.UserId == User.Id))
        });
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("{squadId:guid}/Update")]
    public async Task<IActionResult> UpdateSquad(Guid squadId, SquadUpdateInfoRequest payload)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new { error = "Squad does not exist" });
        }

        if (!squad.UserId.Equals(User.Id))
        {
            return Unauthorized(new { error = "Squad cannot be updated by this user" });
        }

        return Ok(new
        {
            result = (await _squadsRepository.UpdateSquad(squadId, payload.Name, payload.Description,
                payload.ExtendedDescription)).ToString()
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
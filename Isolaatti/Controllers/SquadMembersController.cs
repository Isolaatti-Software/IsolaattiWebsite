using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Repositories;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.DTOs;
using Isolaatti.Enums;
using Isolaatti.Models;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Controllers;

[ApiController]
[Route("/api/Squads/{squadId:guid}")]
public class SquadMembersController : IsolaattiController
{
    private readonly DbContextApp _db;
    private readonly SquadsRepository _squadsRepository;
    public SquadMembersController(DbContextApp db, SquadsRepository squadsRepository)
    {
        _db = db;
        _squadsRepository = squadsRepository;
    }
    
    [IsolaattiAuth]
    [HttpGet]
    [Route("Members")]
    public async Task<IActionResult> GetMembers(Guid squadId, int lastId = -1)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound();
        }

        var members = (from u in _db.Users
            from su in _db.SquadUsers
            where u.Id == su.UserId && su.SquadId == squad.Id && su.Role == SquadUserRole.User
            orderby  u.Id
            select new
            {
                Id = u.Id,
                Name = u.Name,
                ImageId = u.ProfileImageId
            });
        
        if (lastId > 0)
        {
            members = members.Where(member => member.Id > lastId);
        }

        members = members.Take(20);

        return Ok(members);

    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("Admins")]
    public async Task<IActionResult> GetAdmins(Guid squadId, int lastId = -1)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound();
        }

        var members = (from u in _db.Users
            from su in _db.SquadUsers
            where u.Id == su.UserId && su.SquadId == squad.Id && su.Role == SquadUserRole.Admin
            orderby  u.Id
            select new
            {
                Id = u.Id,
                Name = u.Name,
                ImageId = u.ProfileImageId
            });
        
        if (lastId > 0)
        {
            members = members.Where(member => member.Id > lastId);
        }

        members = members.Take(20);

        return Ok(members);
    }
    
    [IsolaattiAuth]
    [HttpGet]
    [Route("Owner")]
    public async Task<IActionResult> GetOwnerOfSquad(Guid squadId)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound();
        }

        var owner = (from u in _db.Users
            where u.Id == squad.UserId
            select new
            {
                Id = u.Id,
                Name = u.Name,
                ImageId = u.ProfileImageId
            }).FirstOrDefault();

        return Ok(owner);
    }
    
    [IsolaattiAuth]
    [HttpPost]
    [Route("Leave")]
    public async Task<IActionResult> LeaveSquad(Guid squadId)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new {result = "squad_not_found"});
        }

        if (squad.UserId == User.Id)
        {
            return BadRequest(new { result = "owner_cannot_leave" });
        }

        var squadUser = _db.SquadUsers.FirstOrDefault(su => su.SquadId == squad.Id && su.UserId == User.Id);
        
        if (squadUser == null)
        {
            return BadRequest(new { result = "user_does_not_belong" });
        }

        _db.SquadUsers.Remove(squadUser);
        await _db.SaveChangesAsync();
        
        return Ok(new {result = "left_squad"});
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("SetOwner")]
    public async Task<IActionResult> SetOwner(Guid squadId, SingleIdentification<int> userId)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new { result = "squad_not_found" });
        }

        if (squad.UserId == userId.Id)
        {
            return Ok(new { result = "ok" });
        }

        if (squad.UserId != User.Id)
        {
            return BadRequest(new { result = "squad_not_owned" });
        }

        

        await _squadsRepository.SetSquadOwner(squad.Id, userId.Id);
        await _squadsRepository.AddUserToSquad(squad.Id, User.Id);
        await _squadsRepository.RemoveAUserFromASquad(squad.Id, userId.Id);

        return Ok(new { result = "ok" });

    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("AddAdmin")]
    public async Task<IActionResult> AddAdmin(Guid squadId, SingleIdentification<int> userId)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new { result = "squad_not_found" });
        }
        if (squad.UserId == userId.Id)
        {
            return BadRequest(new { result = "owner_cannot_be_admin" });
        }

        if (!await _squadsRepository.UserBelongsToSquad(userId.Id, squadId))
        {
            return BadRequest(new {result = "user_is_not_member"});
        }

        var result = await _squadsRepository.SetUserAsAdmin(userId.Id, squadId);

        return result ? Ok() : Problem("Internal error saving to database.");
    }
    
    [IsolaattiAuth]
    [HttpPost]
    [Route("RemoveAdmin")]

    public async Task<IActionResult> RemoveAdmin(Guid squadId, SingleIdentification<int> userId)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad == null)
        {
            return NotFound(new { result = "squad_not_found" });
        }

        if (squad.UserId != User.Id)
        {
            return Unauthorized(new { result = "not_owner"});
        }
        if (squad.UserId == userId.Id)
        {
            return BadRequest(new { result = "owner_cannot_be_user" });
        }

        if (!await _squadsRepository.UserBelongsToSquad(userId.Id, squadId))
        {
            return BadRequest(new {result = "user_is_not_member"});
        }

        var result = await _squadsRepository.SetUserAsNormalUser(userId.Id, squadId);

        return result ? Ok() : Problem("Internal error saving to database.");
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("User/{userId:int}/SetPermissions")]
    public async Task<IActionResult> SetPermissions(Guid squadId, int userId, PermissionDto permissionDto)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad.UserId != User.Id)
        {
            return Unauthorized();
        }
        
        var squadUser = await _db.SquadUsers
            .FirstOrDefaultAsync(su => su.UserId == userId && su.SquadId.Equals(squadId));
        if (squadUser == null)
        {
            return NotFound();
        }

        if (squadUser.Role != SquadUserRole.Admin)
        {
            return BadRequest(new
            {
                error = $"User ${squadUser.UserId} is not admin"
            });
        }

        if (!permissionDto.Validate())
        {
            return BadRequest(new
            {
                error = $"Invalid permissions were specified. Permission can be: {PermissionDto.Permissions}"
            });
        }   
        

        squadUser.Permissions = permissionDto.PermissionsList.Distinct().ToList();
        _db.SquadUsers.Update(squadUser);
        await _db.SaveChangesAsync();
        
        return Ok();
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("User/{userId:int}/GetPermissions")]
    public async Task<IActionResult> GetPermissions(Guid squadId, int userId)
    {
        var squad = await _squadsRepository.GetSquad(squadId);
        if (squad.UserId != User.Id)
        {
            return Unauthorized();
        }
        var squadUser = await _db.SquadUsers
            .FirstOrDefaultAsync(su => su.UserId == userId && su.SquadId.Equals(squadId));
        if (squadUser == null)
        {
            return NotFound();
        }
        if (squadUser.Role != SquadUserRole.Admin)
        {
            return BadRequest(new
            {
                error = $"User ${squadUser.UserId} is not admin"
            });
        }
        

        return Ok(squadUser.Permissions);
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("User/{userId:int}")]
    public async Task<IActionResult> GetSquadUserProfile(Guid squadId, int userId)
    {
        if (!await _squadsRepository.UserBelongsToSquad(User.Id, squadId))
        {
            return Unauthorized();
        }

        var squadUser = await _db.SquadUsers
            .Include(squadUser => squadUser.User)
            .FirstOrDefaultAsync(su => su.UserId == userId && su.SquadId.Equals(squadId));
        if (squadUser == null)
        {
            return NotFound();
        }

        return Ok(new SquadUserDto()
        {
            User = new UserFeed()
            {
                Id = squadUser.UserId,
                Name = squadUser.User.Name,
                ImageId = squadUser.User.ProfileImageId
            },
            Permissions = squadUser.Permissions ?? new List<string>(),
            IsAdmin = squadUser.Role == SquadUserRole.Admin,
            Joined = squadUser.JoinedAt,
            LastInteraction = squadUser.LastInteractionDateTime,
            Ranking = squadUser.Ranking
        });
    }

    [HttpDelete]
    [IsolaattiAuth]
    [Route("RemoveUser")]
    public async Task<IActionResult> RemoveUserFromSquad(Guid squadId, SingleIdentification<int> userId)
    {
        if (!await _squadsRepository.CheckOwner(User.Id, squadId))
        {
            return Unauthorized(new { result = "not_owner"});
        }

        var result = await _squadsRepository.RemoveAUserFromASquad(squadId, userId.Id);
        return result ? Ok(new { result = "success" }) : Problem("Error on saving data");
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("/Squads/AvailablePermissions")]
    public IActionResult AvailablePermissions()
    {
        return Ok(PermissionDto.Permissions);
    }
}
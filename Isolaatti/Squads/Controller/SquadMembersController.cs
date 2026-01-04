using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Enums;
using Isolaatti.Squads.Dto;
using Isolaatti.Squads.Repository;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Squads.Controller;

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
    
    [Authorize]
    [HttpGet]
    [Route("Members")]
    public async Task<IActionResult> GetMembers(Guid squadId, int lastId = -1)
    {
        var squad = await _squadsRepository.GetSquad(squadId, User.Id);
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
    
    [Authorize]
    [HttpGet]
    [Route("User/{userId:int}")]
    public async Task<IActionResult> GetSquadUserProfile(Guid squadId, int userId)
    {
        if (!await _squadsRepository.UserBelongsToSquad(squadId, User.Id))
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
            User = new UserFeedDto()
            {
                Id = squadUser.UserId,
                Name = squadUser.User.Name,
                ImageId = squadUser.User.ProfileImageId
            },
            IsAdmin = false,
            Joined = squadUser.JoinedAt,
            LastInteraction = squadUser.LastInteractionDateTime,
            Ranking = squadUser.Ranking
        });
    }
    
}
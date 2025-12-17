using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Database.EF;
using Isolaatti.Enums;
using Isolaatti.Images.Data;
using Isolaatti.Images.Repository;
using Isolaatti.Squads.Data;
using Isolaatti.Squads.Dto;
using Isolaatti.Utils;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Squads.Repository;

public class SquadsRepository
{
    private readonly DbContextApp _db;
    private readonly ImagesRepository _images;

    public SquadsRepository(DbContextApp dbContextApp, ImagesRepository images)
    {
        _db = dbContextApp;
        _images = images; 
    }

    public async Task<bool> ValidateSquadOwner(Guid squadId, int userId)
    {
        var squad = await _db.Squads.FindAsync(squadId);
        if (squad == null)
        {
            return false;
        }

        return squad.SquadUsers.Any(squadUser => squadUser.UserId == userId && squadUser.Role == SquadUserRole.Owner);
    }
    
    public static Expression<Func<Squad, SquadListItemDto>> ToListItemDto(int userId)
    {
        return squad => new SquadListItemDto
        {
            Id = squad.Id,
            Name = squad.Name,
            Description = squad.Description,
            ExtendedDescription = squad.ExtendedDescription,
            CreationTime = squad.CreationTime,
            ImageId = squad.ImageId,
            PlatformGenerated = squad.PlatformGenerated,
            Keywords = squad.Keywords,
            Membership = squad.SquadUsers
                .Where(userSquad => userSquad.UserId == userId)
                .Select(squadUser => squadUser.Role)
                .FirstOrDefault()
        };
    }
    

    public async Task<SquadCreationResponse> MakeSquad(int userId, SquadCreationRequest squadCreationRequest)
    {
        var userExists = await _db.Users.AnyAsync(user => user.Id == userId);
        if (!userExists)
        {
            return new SquadCreationResponse
            {
                CreationResult = SquadCreationResult.UserDoesNotExist,
                Squad = null
            };
        }

        if (squadCreationRequest.Name.IsNullOrWhiteSpace() 
            || squadCreationRequest.Description.IsNullOrWhiteSpace())
        {
            return new SquadCreationResponse
            {
                CreationResult = SquadCreationResult.ValidationProblems,
                Squad = null
            };
        }
        
        var newSquad = new Squad
        {
            Name = squadCreationRequest.Name,
            Description = squadCreationRequest.Description,
            ExtendedDescription = squadCreationRequest.ExtendedDescription,
            CreationTime = DateTime.Now.ToUniversalTime(),
        };
        
        try
        {
            await _db.Squads.AddAsync(newSquad);
            var squadUser = new SquadUser()
            {
                SquadId = newSquad.Id,
                UserId = userId,
                Role = SquadUserRole.Owner,
                JoinedAt = DateTime.Now.ToUniversalTime()
            };
            await _db.SquadUsers.AddAsync(squadUser);
            await _db.SaveChangesAsync();
            return new SquadCreationResponse
            {
                CreationResult = SquadCreationResult.Success,
                Squad = new SquadListItemDto
                {
                    Id = newSquad.Id,
                    Name = newSquad.Name,
                    Description = newSquad.Description,
                    ExtendedDescription = newSquad.ExtendedDescription,
                    CreationTime = newSquad.CreationTime,
                    ImageId = newSquad.ImageId,
                    Keywords = newSquad.Keywords,
                    PlatformGenerated = newSquad.PlatformGenerated,
                    Membership = SquadUserRole.Owner
                }
            };
        } 
        catch(DbUpdateException)
        {
            return new SquadCreationResponse
            {
                CreationResult = SquadCreationResult.Error,
                Squad = null
            };
        }
    }

    public async Task<bool> UserBelongsToSquad(Guid squadId, int userId)
    {
        return await _db.Squads.AnyAsync(squad =>
            squad.Id == squadId && squad.SquadUsers.Any(squadUser => squadUser.UserId == userId));
    }

    public async Task<SquadListItemDto?> GetSquad(Guid id, int userId)
    {
        return await _db.Squads.Where(squad => squad.Id == id).Select(ToListItemDto(userId)).FirstOrDefaultAsync();
    }

    public async Task<SquadUpdateResult> UpdateSquad(Guid id, int userId, string name, string description, string extendedDescription)
    {
        var squad = await _db.Squads.Where(squad => squad.Id == id && squad.SquadUsers.Any(squadUser => squadUser.UserId == userId && squadUser.Role == SquadUserRole.Owner)).FirstOrDefaultAsync();
        
        if (squad == null)
        {
            return SquadUpdateResult.SquadDoesNotExist;
        }

        if (name.IsNullOrWhiteSpace() || description.IsNullOrWhiteSpace() || extendedDescription.IsNullOrWhiteSpace())
        {
            return SquadUpdateResult.ValidationErrors;
        }

        squad.Name = name;
        squad.Description = description;
        squad.ExtendedDescription = extendedDescription;

        try
        {
            _db.Squads.Update(squad);
            await _db.SaveChangesAsync();
            return SquadUpdateResult.Success;
        }
        catch (DbUpdateException)
        {
            return SquadUpdateResult.Error;
        }
    }

    public async Task<bool> RemoveSquad(Guid squadId, int userId)
    {
        var squad = await _db.Squads
            .Where(squad => squad.Id == squadId && squad.SquadUsers.Any(squadUser =>
                squadUser.UserId == userId && squadUser.Role == SquadUserRole.Owner))
            .FirstOrDefaultAsync();

        if (squad == null)
        {
            return false;
        }

        _db.Squads.Remove(squad);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<AddUserToSquadResult> AddUserToSquad(Guid squadId, int userId)
    {
        var userExists = await _db.Users.AnyAsync(user => user.Id == userId);
        if (!userExists)
        {
            return AddUserToSquadResult.UserDoesNotExist;
        }

        var squad = await _db.Squads.FindAsync(squadId);
        if (squad == null)
        {
            return AddUserToSquadResult.SquadDoesNotExist;
        }

        var newSquadUser = new SquadUser
        {
            SquadId = squad.Id,
            UserId = userId,
            Role = SquadUserRole.User,
            JoinedAt = DateTime.Now.ToUniversalTime()
        };
        
        try
        {
            await _db.SquadUsers.AddAsync(newSquadUser);
            await _db.SaveChangesAsync();
            return AddUserToSquadResult.Success;
        }
        catch (DbUpdateException)
        {
            return AddUserToSquadResult.Error;
        }

    }


    public async Task<bool> RemoveAUserFromASquad(Guid squadId, int userId)
    {
        var squadUser = await _db.SquadUsers
            .Where(squadUser => squadUser.SquadId.Equals(squadId) && squadUser.UserId == userId)
            .SingleOrDefaultAsync();
        
        if (squadUser == null)
            return false;
        
        _db.SquadUsers.Remove(squadUser);
        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return true;
        }
    }
    
    

    public async Task<List<SquadListItemDto>> GetSquads(int userId, Guid? lastId = null, int pageSize = 20)
    {

        if (lastId != null)
        {
            return await _db.Squads
                .Where(squad => squad.Id < lastId)
                .Where(squad => squad.SquadUsers.Any(squadUser => squadUser.UserId == userId))
                .OrderByDescending(squad => squad.Id)
                .Select(ToListItemDto(userId))
                .Take(pageSize + 1)
                .ToListAsync();
        }
        return await _db.Squads
            .Where(squad => squad.SquadUsers.Any(squadUser => squadUser.UserId == userId))
            .OrderByDescending(squad => squad.Id)
            .Select(ToListItemDto(userId))
            .Take(pageSize + 1)
            .ToListAsync();
        
        
    }

    public async Task<IEnumerable<UserFeedDto>> GetMembersOfSquad(Guid squadId)
    {
        // return _db.SquadUsers.Where(squadUser => squadUser.SquadId.Equals(squadId)).Select(u => new UserFeed
        // {
        //     Id = u.UserId,
        //     Name = _db.Users.Find(u.UserId).Name,
        //     ImageId = _db.Users.Find(u.UserId).ProfileImageId
        // });

        var users =
            from squad in _db.Squads
            from squadUser in _db.SquadUsers
            from user in _db.Users
            where squad.Id == squadUser.SquadId && user.Id == squadUser.UserId && squad.Id == squadId
            select new UserFeedDto
            {
                Id = squadUser.UserId,
                Name = user.Name,
                ImageId = user.ProfileImageId
            };
        
        return await users.ToListAsync();
    }

    public async Task<List<SquadListItemDto>> GetSquadsDiscover(int userId, Guid? lastId = null, int pageSize = 20)
    {
        if (lastId != null)
        {
            return await _db.Squads
                .Where(squad => squad.Id < lastId)
                .Where(squad => squad.SquadUsers.Any(squadUser => squadUser.UserId != userId))
                .OrderByDescending(squad => squad.Id)
                .Select(ToListItemDto(userId))
                .Take(pageSize + 1)
                .ToListAsync();
        }
        return await _db.Squads
            .Where(squad => squad.SquadUsers.Any(squadUser => squadUser.UserId != userId))
            .OrderByDescending(squad => squad.Id)
            .Select(ToListItemDto(userId))
            .Take(pageSize + 1)
            .ToListAsync();

    }

    public string? GetSquadName(Guid? squadId)
    {
        return _db.Squads.Where(s => s.Id.Equals(squadId)).Select(s => s.Name).FirstOrDefault();
    }

    public async Task SetSquadImage(Guid squadId, Guid imageId)
    {
        var squad = await _db.Squads.FindAsync(squadId);
        if (squad == null)
        {
            return;
        }

        squad.ImageId = imageId;
        _db.Squads.Update(squad);
        await _db.SaveChangesAsync();
    }
    
    
    
}
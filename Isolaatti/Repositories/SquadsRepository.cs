using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Enums;
using Isolaatti.Helpers;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Repositories;

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
        var squad = await GetSquad(squadId);
        return squad.UserId == userId;
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
            Privacy = squadCreationRequest.Privacy,
            UserId = userId
        };
        
        try
        {
            await _db.Squads.AddAsync(newSquad);
            await _db.SaveChangesAsync();
            return new SquadCreationResponse
            {
                CreationResult = SquadCreationResult.Success,
                Squad = newSquad
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

    public async Task<Squad> GetSquad(Guid id)
    {
        return await _db.Squads.FindAsync(id);
    }

    public async Task<bool> SquadExists(Guid squadId)
    {
        return await _db.Squads.AnyAsync(s => s.Id.Equals(squadId));
    }

    public async Task RemoveSquad(Guid id)
    {
        var squad = await GetSquad(id);
        if (squad == null) return;
        _db.Squads.Remove(squad);
        await _db.SaveChangesAsync();
    }

    public async Task<SquadUpdateResult> UpdateSquad(Guid id, string name, string description, string extendedDescription)
    {
        var squad = await GetSquad(id);
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
    
    public async Task UpdatePrivacy(Guid id, SquadPrivacy privacy)
    {
        var squad = await _db.Squads.FindAsync(id);
        if (squad == null)
            return;
        squad.Privacy = privacy;
        _db.Squads.Update(squad);
        await _db.SaveChangesAsync();
    }

    public async Task<AddUserToSquadResult> AddUserToSquad(Guid squadId, int userId)
    {
        var userExists = await _db.Users.AnyAsync(user => user.Id == userId);
        if (!userExists)
        {
            return AddUserToSquadResult.UserDoesNotExist;
        }
        
        var squad = await GetSquad(squadId);
        if (squad == null)
        {
            return AddUserToSquadResult.SquadDoesNotExist;
        }

        var userWasAddedAlready = 
            await  _db.SquadUsers.AnyAsync(squadUser => squadUser.SquadId == squad.Id && squadUser.UserId == userId);
        if (userWasAddedAlready || squad.UserId == userId)
        {
            return AddUserToSquadResult.AlreadyInSquad;
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

    public async Task<bool> UserBelongsToSquad(int userId, Guid squadId)
    {
        var userExists = await _db.Users.AnyAsync(user => user.Id == userId);
        if (!userExists)
        {
            return false;
        }
        
        var squad = await GetSquad(squadId);
        if (squad == null)
        {
            return false;
        }

        return await _db.SquadUsers
            .AnyAsync(squadUser => squadUser.UserId.Equals(userId) && squadUser.SquadId.Equals(squadId)) || squad.UserId == userId;
    }

    public async Task RemoveAUserFromASquad(Guid squadId, int userId)
    {
        var squadUser = await _db.SquadUsers
            .Where(squadUser => squadUser.SquadId.Equals(squadId) && squadUser.UserId == userId)
            .SingleOrDefaultAsync();
        
        if (squadUser == null)
            return;
        
        _db.SquadUsers.Remove(squadUser);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAllUsersFromASquad(Guid squadId)
    {
        var squadUsers = _db.SquadUsers
            .Where(squadUser => squadUser.SquadId.Equals(squadId));
        
        _db.SquadUsers.RemoveRange(squadUsers);
        await _db.SaveChangesAsync();
    }

    public IEnumerable<Squad> GetSquadUserBelongs(int userId, Guid? lastId = null)
    {
        return from squadUser in _db.SquadUsers
            from squad in _db.Squads
            where squadUser.UserId == userId && squad.Id == squadUser.SquadId
            select squad;
    }

    public IQueryable<Squad> GetSquadsUserAdmins(int userId, Guid? lastId = null)
    {
        return  _db.Squads
            .Where(squad => squad.UserId == userId);
    }

    public async Task<IEnumerable<UserFeed>> GetMembersOfSquad(Guid squadId)
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
            select new UserFeed
            {
                Id = squadUser.UserId,
                Name = user.Name,
                ImageId = user.ProfileImageId
            };
        
        return await users.ToListAsync();
    }

    public string GetSquadName(Guid? squadId)
    {
        return _db.Squads.Where(s => s.Id.Equals(squadId)).Select(s => s.Name).FirstOrDefault();
    }

    public async Task SetSquadImage(Guid squadId, string imageId)
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

    public async Task<IEnumerable<Image>> GetImagesOfSquad(Guid squadId, string lastId)
    {
        return await _images.GetImagesOfSquad(squadId, lastId);
    }

    public async Task SetSquadPrivacy(Guid squadId, SquadPrivacy privacy) 
    {
        var squad = await _db.Squads.FindAsync(squadId);
        if(squad == null)
        {
            return;
        }

        squad.Privacy = privacy;

        _db.Squads.Update(squad);
        await _db.SaveChangesAsync();
    }

    public async Task SetSquadOwner(Guid squadId, int userId)
    {
        var squad = await _db.Squads.FindAsync(squadId);
        if (squad == null)
        {
            return;
        }

        squad.UserId = userId;
        _db.Squads.Update(squad);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> SetUserAsAdmin(int userId, Guid squadId)
    {
        var squad = await _db.Squads.FindAsync(squadId);
        if (squad == null)
        {
            return false;
        }

        var squadUser = await _db.SquadUsers.FirstOrDefaultAsync(su => su.UserId == userId);
        if (squadUser == null)
        {
            return false;
        }

        squadUser.Role = SquadUserRole.Admin;

        _db.SquadUsers.Update(squadUser);

        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            // TODO Log this
            return false;
        }
    }

    public async Task<bool> SetUserAsNormalUser(int userId, Guid squadId)
    {
        var squad = await _db.Squads.FindAsync(squadId);
        if (squad == null)
        {
            return false;
        }

        var squadUser = await _db.SquadUsers.FirstOrDefaultAsync(su => su.UserId == userId);
        if (squadUser == null)
        {
            return false;
        }

        squadUser.Role = SquadUserRole.User;

        _db.SquadUsers.Update(squadUser);

        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            // TODO Log this
            return false;
        }
    }
    
}
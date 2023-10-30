using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.DTOs;
using Isolaatti.Enums;
using Isolaatti.Models;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Repositories;

public class SquadUsersRepository
{
    private readonly DbContextApp _db;

    public SquadUsersRepository(DbContextApp dbContextApp)
    {
        _db = dbContextApp;
    }

    public async Task<UserSearchFeed> SearchOnUsers(string text, Guid squadId, int lastId = 0)
    {
        var query = (from user in _db.Users
            from squadUser in _db.SquadUsers
            where user.Id > lastId 
                  && user.Id == squadUser.UserId
                  && squadUser.SquadId.Equals(squadId)
                  && (user.Name.Contains(text) || user.Email.Contains(text))
            orderby user.Id
            select new RankedSquadUser
            {
                User = new UserFeedDto
                {
                    Id = user.Id,
                    ImageId = user.ProfileImageId,
                    Name = user.Name
                },
                Ranking = squadUser.Ranking
            }).Take(20);
        var last = query.LastOrDefault();
        return new UserSearchFeed()
        {
            Users = await query
                .OrderByDescending(rankedSquadUser => rankedSquadUser.Ranking)
                .ToListAsync(),
            LastId = last?.User.Id
        };
    }

    public async Task<UserSearchFeed> GetRankedSuggestions(Guid squadId,bool owner = true, bool admins = true, bool normalMembers = true)
    {
        var query = (from user in _db.Users
            from squadUser in _db.SquadUsers
            where (user.Id == squadUser.UserId)
                  && squadUser.SquadId.Equals(squadId)
                  && ((squadUser.Role == SquadUserRole.Admin && admins) || (squadUser.Role == SquadUserRole.User && normalMembers))
            orderby squadUser.Ranking
            select new RankedSquadUser
            {
                User = new UserFeedDto
                {
                    Id = user.Id,
                    ImageId = user.ProfileImageId,
                    Name = user.Name
                },
                Ranking = squadUser.Ranking
            }).Take(5);

        return new UserSearchFeed
        {
            Users = query.ToList()
        };
    }

    public async Task<List<UserFeedDto>> GetAdminsOfSquad(Guid squadId)
    {
        return await (from user in _db.Users
            from squadUser in _db.SquadUsers
            where user.Id == squadUser.UserId && squadUser.SquadId.Equals(squadId) &&
                  squadUser.Role == SquadUserRole.Admin
            select new UserFeedDto
            {
                Id = user.Id,
                Name = user.Name,
                ImageId = user.ProfileImageId
            }).ToListAsync();
    }
}
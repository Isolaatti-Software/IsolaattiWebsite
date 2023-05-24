using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Enums;
using Isolaatti.Models;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Repositories;

public class SquadPermissionsRepository
{
    private readonly DbContextApp _db;

    public SquadPermissionsRepository(DbContextApp db)
    {
        _db = db;
    }

    public async Task<bool> EvaluatePermissionsForAdmin(Guid squadId, int userId, params string[] permissionToEvaluate)
    {
        var squadUser = await _db.SquadUsers.FirstOrDefaultAsync(su => su.UserId == userId && su.SquadId.Equals(squadId));
        return squadUser.Role == SquadUserRole.Admin && permissionToEvaluate.ToList().TrueForAll(squadUser.Permissions.Contains);
    }
}
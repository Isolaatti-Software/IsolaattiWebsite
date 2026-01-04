using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Database.EF;
using Isolaatti.Enums;
using Isolaatti.Squads.Dto;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Squads.Repository;

public class SquadUsersRepository
{
    private readonly DbContextApp _db;

    public SquadUsersRepository(DbContextApp dbContextApp)
    {
        _db = dbContextApp;
    }
}
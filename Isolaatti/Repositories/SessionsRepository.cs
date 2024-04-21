using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.SignIn.Data;
using Isolaatti.Accounts.SignIn.Dto;
using Isolaatti.Database.EF;

namespace Isolaatti.Repositories;

public class SessionsRepository(DbContextApp db)
{
    public async Task<Session> InsertSession(Session session)
    {
        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }

    public IEnumerable<Session> FindSessionsOfUser(int userId)
    {
        return db.Sessions.Where(session => session.UserId == userId).ToList();
    }

    public async Task<int?> FindUserIdFromSession(SessionDto sessionDto)
    {
        var session = await db.Sessions.FindAsync(sessionDto.SessionId);

        if(session == null) 
        {
            return null;
        }

        if(!session.SessionKey.Equals(sessionDto.SessionKey))
        {
            return null;
        }

        return session.UserId;
    }

    public async Task<bool> RemoveSession(SessionDto sessionDto)
    {
        var session = await db.Sessions.FindAsync(sessionDto.SessionId);
        if (session == null)
        {
            return false;
        }
        db.Sessions.Remove(session);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveSessionsByUserId(int userId, IEnumerable<Guid> exceptIds)
    {
        db.Sessions.RemoveRange(db.Sessions.Where(session => session.UserId == userId && !exceptIds.Contains(session.Id)));
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<Session?> FindSessionById(SessionDto sessionDto)
    {
        return await db.Sessions.FindAsync(sessionDto.SessionId);
    }

    public async Task<bool> RemoveSessions(int userId, IEnumerable<Guid> ids)
    {
        db.Sessions.RemoveRange(db.Sessions.Where(session => session.UserId == userId && ids.Contains(session.Id)));
        await db.SaveChangesAsync();
        return true;
    }
}
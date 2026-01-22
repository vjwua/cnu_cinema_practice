using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SessionRepository(CinemaDbContext context) : ISessionRepository
{
    public async Task<IEnumerable<Session>> GetAllAsync()
    {
        return await context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .ToListAsync();
    }

    public async Task<Session?> GetByIdAsync(int id)
    {
        return await context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Session>> GetByMovieIdAsync(int movieId)
    {
        return await context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.MovieId == movieId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetByHallIdAsync(int hallId)
    {
        return await context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.HallId == hallId)
            .ToListAsync();
    }

    public async Task<Session?> GetByIdWithSeatsAsync(int id)
    {
        return await context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .ThenInclude(h => h.Seats)
            .ThenInclude(seat => seat.SeatType)
            .Include(s => s.SeatReservations)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Session>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.StartTime >= start && s.StartTime <= end)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetUpcomingSessionsAsync()
    {
        var now = DateTime.UtcNow;
        return await context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.StartTime > now)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<bool> HasAnyOrdersAsync(int sessionId)
    {
        return await context.Orders.AnyAsync(o => o.SessionId == sessionId);
    }

    public async Task<Session> CreateAsync(Session session)
    {
        context.Sessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }

    public async Task UpdateAsync(Session session)
    {
        context.Sessions.Update(session);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var session = await context.Sessions.FindAsync(id);
        if (session != null)
        {
            context.Sessions.Remove(session);
            await context.SaveChangesAsync();
        }
    }
}
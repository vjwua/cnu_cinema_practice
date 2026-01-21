using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly CinemaDbContext _context;

    public SessionRepository(CinemaDbContext context)
    {
        _context = context;
    }

    public async Task<Session?> GetByIdAsync(int id)
    {
        return await _context.Sessions.FindAsync(id);
    }

    public async Task<Session?> GetByIdWithMovieAndHallAsync(int id)
    {
        return await _context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Session>> GetByMovieIdAsync(int movieId)
    {
        return await _context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.MovieId == movieId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetByHallIdAsync(int hallId)
    {
        return await _context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.HallId == hallId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.StartTime >= start && s.StartTime <= end)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetUpcomingSessionsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.StartTime > now)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<bool> HasAnyOrdersAsync(int sessionId)
    {
        return await _context.Orders.AnyAsync(o => o.SessionId == sessionId);
    }

    // public async Task<Session?> GetByIdWithHallSeatsAndTicketsAsync(int id)
    // {
    //     return await _context.Sessions
    //         .Include(s => s.Hall)
    //         .ThenInclude(h => h.Seats)
    //         .Include(s => s.Orders)
    //         .ThenInclude(o => o.Tickets)
    //         .Include(s => s.Movie)
    //         .FirstOrDefaultAsync(s => s.Id == id);
    // }


    public async Task<Session> CreateAsync(Session session)
    {
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task UpdateAsync(Session session)
    {
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var session = await _context.Sessions.FindAsync(id);
        if (session != null)
        {
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
        }
    }
}
using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(int id);
    Task<Session?> GetByIdWithSeatsAsync(int id); // для перегляду місць
    Task<Session?> GetByIdWithMovieAndHallAsync(int id);
    Task<IEnumerable<Session>> GetByMovieIdAsync(int movieId);
    Task<IEnumerable<Session>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<Session>> GetUpcomingSessionsAsync(); // для афіші
    Task<Session> CreateAsync(Session session);
    Task UpdateAsync(Session session);
    Task DeleteAsync(int id);
    Task<bool> HasConflictAsync(int hallId, DateTime startTime, int durationMinutes);
}
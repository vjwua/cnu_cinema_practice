using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(int id);
    Task<Session?> GetByIdWithMovieAndHallAsync(int id);
    Task<IEnumerable<Session>> GetByMovieIdAsync(int movieId);
    Task<IEnumerable<Session>> GetByHallIdAsync(int hallId);
    Task<IEnumerable<Session>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<Session>> GetUpcomingSessionsAsync();
    Task<bool> HasAnyOrdersAsync(int sessionId);
    Task<Session> CreateAsync(Session session);
    Task UpdateAsync(Session session);
    Task DeleteAsync(int id);
}
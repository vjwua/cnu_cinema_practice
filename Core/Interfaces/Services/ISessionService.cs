using Core.DTOs.Sessions;

namespace Core.Interfaces.Services;

public interface ISessionService
{
    // Task<SessionDetailDTO?> GetSessionDetailsByIdAsync(int id);
    Task<IEnumerable<SessionListDTO>> GetSessionsByMovieIdAsync(int movieId);
    Task<IEnumerable<SessionListDTO>> GetSessionsByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<SessionPreviewDTO>> GetUpcomingSessionsAsync();
    // Task<SessionDetailDTO?> GetSessionWithMovieAndHallAsync(int sessionId);

    Task<bool> HasScheduleConflictAsync(int hallId, DateTime startTime, int durationMinutes,
        int? excludeSessionId = null);

    Task DeleteSessionAsync(int id);

    Task UpdateSessionAsync(UpdateSessionDTO dto);
    // Task<SessionDetailDTO> CreateSessionAsync(CreateSessionDTO dto);
}
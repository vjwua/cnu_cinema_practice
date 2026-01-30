using Core.DTOs.Sessions;
using Core.DTOs.Common;

namespace Core.Interfaces.Services;

public interface ISessionService
{
    Task<SessionDetailDTO?> GetSessionByIdAsync(int id);
    Task<IEnumerable<SessionListDTO>> GetAllSessionsAsync();
    Task<PagedResult<SessionListDTO>> GetAdminPagedAsync(SessionAdminQueryDTO query);
    Task<IEnumerable<SessionListDTO>> GetSessionsByMovieIdAsync(int movieId);
    Task<IEnumerable<SessionListDTO>> GetSessionsByHallIdAsync(int hallId);
    Task<IEnumerable<SessionListDTO>> GetSessionsByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<SessionPreviewDTO>> GetUpcomingSessionsAsync();
    Task<SessionDetailDTO?> GetSessionByIdWithSeatsAsync(int sessionId);

    Task<bool> HasScheduleConflictAsync(int hallId, DateTime startTime, int durationMinutes,
        int? excludeSessionId = null);

    Task DeleteSessionAsync(int id);

    Task UpdateSessionAsync(int id, UpdateSessionDTO dto);
    Task<SessionDetailDTO> CreateSessionAsync(CreateSessionDTO dto);
}
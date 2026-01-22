using AutoMapper;
using Core.DTOs.Sessions;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using FluentValidation;
using Core.Validators.Sessions;

namespace Core.Services;

public class SessionService(
    ISessionRepository sessionRepository,
    IMapper mapper,
    IValidator<CreateSessionDTO> createValidator,
    IValidator<UpdateSessionDTO> updateValidator,
    SessionBusinessValidator sessionBusinessValidator,
    CreateSessionBusinessValidator createBusinessValidator,
    UpdateSessionBusinessValidator updateBusinessValidator)
    : ISessionService
{
    public async Task<SessionDetailDTO?> GetSessionByIdAsync(int id)
    {
        var session = await sessionRepository.GetByIdAsync(id);
        return session == null ? null : mapper.Map<SessionDetailDTO>(session);
    }

    public async Task<IEnumerable<SessionListDTO>> GetAllSessionsAsync()
    {
        var sessions = await sessionRepository.GetAllAsync();
        return sessions.Select(mapper.Map<SessionListDTO>);
    }

    public async Task<IEnumerable<SessionListDTO>> GetSessionsByMovieIdAsync(int movieId)
    {
        var sessions = await sessionRepository.GetByMovieIdAsync(movieId);
        return sessions.Select(mapper.Map<SessionListDTO>);
    }

    public async Task<IEnumerable<SessionListDTO>> GetSessionsByHallIdAsync(int hallId)
    {
        var sessions = await sessionRepository.GetByHallIdAsync(hallId);
        return sessions.Select(mapper.Map<SessionListDTO>);
    }

    public async Task<IEnumerable<SessionListDTO>> GetSessionsByDateRangeAsync(DateTime start, DateTime end)
    {
        var sessions = await sessionRepository.GetByDateRangeAsync(start, end);
        return sessions.Select(mapper.Map<SessionListDTO>);
    }

    public async Task<IEnumerable<SessionPreviewDTO>> GetUpcomingSessionsAsync()
    {
        var sessions = await sessionRepository.GetUpcomingSessionsAsync();
        return sessions.Select(mapper.Map<SessionPreviewDTO>);
    }

    public async Task<SessionDetailDTO?> GetSessionByIdWithSeatsAsync(int sessionId)
    {
        var session = await sessionRepository.GetByIdWithSeatsAsync(sessionId);
        if (session == null) return null;

        var dto = mapper.Map<SessionDetailDTO>(session);
        dto.Seats = MapSeatsWithAvailability(session);
        return dto;
    }

    public async Task<SessionDetailDTO> CreateSessionAsync(CreateSessionDTO dto)
    {
        await createValidator.ValidateAndThrowAsync(dto);
        await createBusinessValidator.ValidateAsync(dto);

        var session = mapper.Map<Session>(dto);
        var created = await sessionRepository.CreateAsync(session);

        var fullSession = await sessionRepository.GetByIdAsync(created.Id);
        return fullSession == null
            ? throw new InvalidOperationException($"Failed to retrieve created session with id {created.Id}")
            : mapper.Map<SessionDetailDTO>(fullSession);
    }

    public async Task UpdateSessionAsync(UpdateSessionDTO dto)
    {
        await updateValidator.ValidateAndThrowAsync(dto);
        var existing = await updateBusinessValidator.ValidateAsync(dto);

        existing.MovieId = dto.MovieId ?? existing.MovieId;
        existing.HallId = dto.HallId ?? existing.HallId;
        existing.StartTime = dto.StartTime ?? existing.StartTime;
        existing.BasePrice = dto.BasePrice ?? existing.BasePrice;
        existing.MovieFormat = dto.MovieFormat ?? existing.MovieFormat;

        await sessionRepository.UpdateAsync(existing);
    }

    public async Task DeleteSessionAsync(int id)
    {
        await sessionBusinessValidator.ValidateSessionExistsAsync(id);
        await sessionBusinessValidator.ValidateCanDeleteAsync(id);
        await sessionRepository.DeleteAsync(id);
    }

    public async Task<bool> HasScheduleConflictAsync(
        int hallId,
        DateTime startTime,
        int durationMinutes,
        int? excludeSessionId = null)
    {
        return await sessionBusinessValidator.HasScheduleConflictAsync(
            hallId,
            startTime,
            durationMinutes,
            excludeSessionId);
    }

    private static List<SessionSeatDTO> MapSeatsWithAvailability(Session session)
    {
        var now = DateTime.UtcNow;
        return session.Hall.Seats
            .Select(seat => new SessionSeatDTO
            {
                SeatId = seat.Id,
                RowNum = seat.RowNum,
                SeatNum = seat.SeatNum,
                SeatType = seat.SeatType.Name,
                AddedPrice = seat.SeatType.AddedPrice,
                IsAvailable = !session.SeatReservations
                    .Any(r => r.SeatId == seat.Id &&
                              (r.Status == ReservationStatus.Sold ||
                               (r.Status == ReservationStatus.Reserved &&
                                (r.ExpiresAt == null || r.ExpiresAt > now))))
            }).ToList();
    }
}
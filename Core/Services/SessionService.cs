using AutoMapper;
using Core.DTOs.Seats;
using Core.DTOs.Sessions;
using Core.Entities;
using Core.Interfaces.Services;
using FluentValidation;
using Infrastructure.Repositories.Interfaces;
using Core.Validators.Sessions;

namespace Core.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateSessionDTO> _createValidator;
    private readonly IValidator<UpdateSessionDTO> _updateValidator;
    private readonly SessionBusinessValidator _sessionBusinessValidator;
    private readonly CreateSessionBusinessValidator _createBusinessValidator;
    private readonly UpdateSessionBusinessValidator _updateBusinessValidator;

    public SessionService(
        ISessionRepository sessionRepository,
        IMapper mapper,
        IValidator<CreateSessionDTO> createValidator,
        IValidator<UpdateSessionDTO> updateValidator,
        SessionBusinessValidator sessionBusinessValidator,
        CreateSessionBusinessValidator createBusinessValidator,
        UpdateSessionBusinessValidator updateBusinessValidator)
    {
        _sessionRepository = sessionRepository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _sessionBusinessValidator = sessionBusinessValidator;
        _createBusinessValidator = createBusinessValidator;
        _updateBusinessValidator = updateBusinessValidator;
    }

    // public async Task<SessionDetailDTO?> GetSessionDetailsByIdAsync(int id)
    // {
    //     var session = await _sessionRepository.GetByIdWithHallSeatsAndTicketsAsync(id);
    //     if (session == null) return null;
    //
    //     var dto = _mapper.Map<SessionDetailDTO>(session);
    //     dto.Seats = BuildSeats(session);
    //     return dto;
    // }
    //
    // public async Task<SessionDetailDTO?> GetSessionWithMovieAndHallAsync(int sessionId)
    // {
    //     var session = await _sessionRepository.GetByIdWithHallSeatsAndTicketsAsync(sessionId);
    //     if (session == null) return null;
    //
    //     var dto = _mapper.Map<SessionDetailDTO>(session);
    //     dto.Seats = BuildSeats(session);
    //     return dto;
    // }

    public async Task<IEnumerable<SessionListDTO>> GetSessionsByMovieIdAsync(int movieId)
    {
        var sessions = await _sessionRepository.GetByMovieIdAsync(movieId);
        return sessions.Select(s => _mapper.Map<SessionListDTO>(s));
    }

    public async Task<IEnumerable<SessionListDTO>> GetSessionsByDateRangeAsync(DateTime start, DateTime end)
    {
        var sessions = await _sessionRepository.GetByDateRangeAsync(start, end);
        return sessions.Select(s => _mapper.Map<SessionListDTO>(s));
    }

    public async Task<IEnumerable<SessionPreviewDTO>> GetUpcomingSessionsAsync()
    {
        var sessions = await _sessionRepository.GetUpcomingSessionsAsync();
        return sessions.Select(s => _mapper.Map<SessionPreviewDTO>(s));
    }

    // public async Task<SessionDetailDTO> CreateSessionAsync(CreateSessionDTO dto)
    // {
    //     await _createValidator.ValidateAndThrowAsync(dto);
    //     await _createBusinessValidator.ValidateAsync(dto);
    //
    //     var entity = _mapper.Map<Session>(dto);
    //     var created = await _sessionRepository.CreateAsync(entity);
    //
    //     var sessionWithRefs = await _sessionRepository.GetByIdWithHallSeatsAndTicketsAsync(created.Id);
    //     if (sessionWithRefs == null)
    //         throw new InvalidOperationException($"Failed to retrieve created session {created.Id}");
    //
    //     var detail = _mapper.Map<SessionDetailDTO>(sessionWithRefs);
    //
    //     // all seats are available for new session
    //     detail.Seats = BuildSeats(sessionWithRefs);
    //
    //     return detail;
    // }

    public async Task UpdateSessionAsync(UpdateSessionDTO dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);
        var existing = await _updateBusinessValidator.ValidateAsync(dto);

        existing.MovieId = dto.MovieId ?? existing.MovieId;
        existing.HallId = dto.HallId ?? existing.HallId;
        existing.StartTime = dto.StartTime ?? existing.StartTime;
        existing.BasePrice = dto.BasePrice ?? existing.BasePrice;
        existing.MovieFormat = dto.MovieFormat ?? existing.MovieFormat;

        await _sessionRepository.UpdateAsync(existing);
    }

    public async Task DeleteSessionAsync(int id)
    {
        await _sessionBusinessValidator.ValidateSessionExistsAsync(id);
        await _sessionBusinessValidator.ValidateCanDeleteAsync(id);
        await _sessionRepository.DeleteAsync(id);
    }

    public async Task<bool> HasScheduleConflictAsync(int hallId, DateTime startTime, int durationMinutes,
        int? excludeSessionId = null)
    {
        return await _sessionBusinessValidator.HasScheduleConflictAsync(hallId, startTime, durationMinutes,
            excludeSessionId);
    }

    // private List<SeatDTO> BuildSeats(Session session)
    // {
    //     var soldSeatIds = session.Orders?
    //                           .SelectMany(o => o.Tickets)
    //                           .Select(t => t.SeatId)
    //                           .ToHashSet()
    //                       ?? new HashSet<int>();
    //
    //     return session.Hall.Seats
    //         .Select(seat => new SeatDTO
    //         {
    //             Id = seat.Id,
    //             Row = seat.RowNum,
    //             Number = seat.SeatNum,
    //             IsAvailable = !soldSeatIds.Contains(seat.Id)
    //         })
    //         .OrderBy(s => s.Row)
    //         .ThenBy(s => s.Number)
    //         .ToList();
    // }
}
using Core.DTOs.Sessions;
using Core.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Core.Validators.Sessions;

public class UpdateSessionBusinessValidator(
    SessionBusinessValidator businessValidator,
    IMovieRepository movieRepository)
{
    public async Task<Session> ValidateAsync(UpdateSessionDTO dto)
    {
        var existingSession = await businessValidator.ValidateSessionExistsAsync(dto.Id);

        if (dto.MovieId.HasValue)
            await businessValidator.ValidateMovieExistsAsync(dto.MovieId.Value);

        if (dto.HallId.HasValue)
            await businessValidator.ValidateHallExistsAsync(dto.HallId.Value);

        await ValidateScheduleIfNeededAsync(dto, existingSession);

        return existingSession;
    }

    private async Task ValidateScheduleIfNeededAsync(UpdateSessionDTO dto, Session existingSession)
    {
        bool needsScheduleCheck = dto.HallId.HasValue || dto.StartTime.HasValue || dto.MovieId.HasValue;
        if (!needsScheduleCheck)
            return;

        var hallId = dto.HallId ?? existingSession.HallId;
        var startTime = dto.StartTime ?? existingSession.StartTime;

        var movie = dto.MovieId.HasValue
            ? await movieRepository.GetByIdAsync(dto.MovieId.Value)
            : existingSession.Movie;

        var duration = movie?.DurationMinutes ?? 0;

        await businessValidator.ValidateNoScheduleConflictAsync(hallId, startTime, duration, dto.Id);
    }
}
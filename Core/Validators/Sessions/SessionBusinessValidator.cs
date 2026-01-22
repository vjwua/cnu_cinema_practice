using Core.Entities;
using Core.Interfaces.Repositories;
using FluentValidation;

namespace Core.Validators.Sessions;

public class SessionBusinessValidator(
    IMovieRepository movieRepository,
    IHallRepository hallRepository,
    ISessionRepository sessionRepository)
{
    public async Task<Movie> ValidateMovieExistsAsync(int movieId)
    {
        var movie = await movieRepository.GetByIdAsync(movieId);
        return movie ?? throw new ValidationException($"Movie with id {movieId} was not found");
    }

    public async Task<Entities.Hall> ValidateHallExistsAsync(int hallId)
    {
        var hall = await hallRepository.GetByIdAsync(hallId);
        return hall ?? throw new ValidationException($"Hall with id {hallId} was not found");
    }

    public async Task<Session> ValidateSessionExistsAsync(int sessionId)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId);
        return session ?? throw new ValidationException($"Session with id {sessionId} was not found");
    }

    public async Task ValidateNoScheduleConflictAsync(
        int hallId,
        DateTime startTime,
        int durationMinutes,
        int? excludeSessionId = null)
    {
        if (await HasScheduleConflictAsync(hallId, startTime, durationMinutes, excludeSessionId))
        {
            throw new ValidationException(
                "Schedule conflict detected. The hall is already occupied at this time");
        }
    }

    public async Task ValidateCanDeleteAsync(int sessionId)
    {
        if (await sessionRepository.HasAnyOrdersAsync(sessionId))
        {
            throw new ValidationException(
                "Cannot delete a session that has existing orders");
        }
    }

    public async Task<bool> HasScheduleConflictAsync(
        int hallId,
        DateTime startTime,
        int durationMinutes,
        int? excludeSessionId)
    {
        var sessions = await sessionRepository.GetByHallIdAsync(hallId);
        var newEnd = startTime.AddMinutes(durationMinutes);

        return (from session in sessions
            where !excludeSessionId.HasValue || session.Id != excludeSessionId.Value
            let existingEnd = session.StartTime.AddMinutes(session.Movie.DurationMinutes)
            where startTime < existingEnd && newEnd > session.StartTime
            select session).Any();
    }
}
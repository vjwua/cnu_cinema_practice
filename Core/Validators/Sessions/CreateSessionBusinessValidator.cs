using Core.DTOs.Sessions;

namespace Core.Validators.Sessions;

public class CreateSessionBusinessValidator(SessionBusinessValidator businessValidator)
{
    public async Task ValidateAsync(CreateSessionDTO dto)
    {
        var movie = await businessValidator.ValidateMovieExistsAsync(dto.MovieId);
        await businessValidator.ValidateHallExistsAsync(dto.HallId);
        await businessValidator.ValidateNoScheduleConflictAsync(
            dto.HallId,
            dto.StartTime,
            movie.DurationMinutes);
    }
}
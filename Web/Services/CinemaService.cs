using cnu_cinema_practice.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace cnu_cinema_practice.Services;

public class CinemaService : ICinemaService
{
    private readonly CinemaDbContext _context;

    public CinemaService(CinemaDbContext context)
    {
        _context = context;
    }

    public async Task<List<cnu_cinema_practice.Models.Movie>> GetMoviesAsync()
    {
        var dbMovies = await _context.Movies
            .Include(m => m.Sessions)
            .ThenInclude(s => s.Hall)
            .ToListAsync();

        return dbMovies.Select(MapToViewModel).ToList();
    }

    public async Task<cnu_cinema_practice.Models.Movie?> GetMovieByIdAsync(string id)
    {
        if (!int.TryParse(id, out int dbId)) return null;

        var dbMovie = await _context.Movies
            .Include(m => m.Sessions)
            .ThenInclude(s => s.Hall)
            .FirstOrDefaultAsync(m => m.Id == dbId);

        return dbMovie == null ? null : MapToViewModel(dbMovie);
    }

    public Task<List<CinemaLocation>> GetCinemasAsync()
    {
        // For now, return static data as Cinemas table might not exist or be different
        return Task.FromResult(new List<CinemaLocation>
        {
            new CinemaLocation { Id = 1, Name = "CineMaster Center", Address = "вул. Хрещатик, 24", Phone = "+380 44 234 56 78", HallCount = 12, Rating = 4.9 },
            new CinemaLocation { Id = 2, Name = "CineMaster River Mall", Address = "Дніпровська набережна, 12", Phone = "+380 44 567 89 01", HallCount = 10, Rating = 4.8 }
        });
    }

    private cnu_cinema_practice.Models.Movie MapToViewModel(Core.Entities.Movie dbMovie)
    {
        return new cnu_cinema_practice.Models.Movie
        {
            Id = dbMovie.Id.ToString(),
            Title = dbMovie.Name,
            Genre = ConvertGenre(dbMovie.Genre),
            Rating = (double)(dbMovie.ImdbRating ?? 0),
            Duration = dbMovie.DurationMinutes,
            Poster = dbMovie.PosterUrl ?? "https://placehold.co/600x900?text=No+Poster",
            Backdrop = "https://placehold.co/1200x800?text=No+Backdrop", // Fallback as DB might not have backdrop
            Description = dbMovie.Description ?? "Опис відсутній",
            AgeLimit = dbMovie.AgeLimit + "+",
            Director = dbMovie.Director ?? "Невідомо",
            Actors = new List<string>(), // DB doesn't have actors yet
            Language = "Українська",
            Sessions = dbMovie.Sessions.Select(s => new cnu_cinema_practice.Models.Session
            {
                Id = s.Id.ToString(),
                Date = s.StartTime.ToString("yyyy-MM-dd"),
                Time = s.StartTime.ToString("HH:mm"),
                Price = s.BasePrice,
                HallName = s.Hall.Name,
                HallType = "Standard", // Simplification
                OccupiedSeats = new List<string>() // Need logic for this later
            }).ToList()
        };
    }

    // Simple helper to convert byte genre to list of strings (simplified logic)
    private List<string> ConvertGenre(byte genreId)
    {
        // This is a placeholder. You should likely map your Core.Enums.Genre here
        return new List<string> { "Фільм" };
    }
}

using Core.Entities;
using Core.Enums;

namespace Infrastructure.Repositories.Interfaces;

public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(int id);
    Task<IEnumerable<Movie>> GetByGenreAsync(MovieGenre genre);
    Task<Movie> CreateAsync(Movie movie);
    Task UpdateAsync(Movie movie);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<Movie>> SearchByNameAsync(string searchTerm);
}
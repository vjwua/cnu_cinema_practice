using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Repositories;

public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(int id);
    Task<IEnumerable<Movie>> GetByGenreAsync(MovieGenre genre);
    Task<Movie> CreateAsync(Movie movie);
    Task UpdateAsync(Movie movie);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<Movie>> SearchByNameAsync(string searchTerm);
}
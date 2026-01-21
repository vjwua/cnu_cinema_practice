using Core.DTOs.Movies;
using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Services;

public interface IMovieService
{
    Task<IEnumerable<MovieListDTO>> GetAllAsync();
    Task<MovieDetailDTO?> GetByIdAsync(int id);
    Task<IEnumerable<MovieDetailDTO>> GetByGenreAsync(MovieGenre genre);
    Task<MovieDetailDTO> CreateAsync(Movie movie);
    Task UpdateAsync(Movie movie);
    Task DeleteAsync(Movie movie);
    Task<IEnumerable<MovieListDTO>> SearchByNameAsync(string searchTerm);
}
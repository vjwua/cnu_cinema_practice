using cnu_cinema_practice.Models;

namespace cnu_cinema_practice.Services;

public interface ICinemaService
{
    Task<List<cnu_cinema_practice.Models.Movie>> GetMoviesAsync();
    Task<cnu_cinema_practice.Models.Movie?> GetMovieByIdAsync(string id);
    Task<List<CinemaLocation>> GetCinemasAsync();
}

using Core.DTOs.External;

namespace Core.Interfaces.Services;

public interface IExternalMovieService
{
    Task<IEnumerable<ExternalMovieSearchResultDTO>> SearchMoviesAsync(string query);
    Task<ExternalMovieDetailDTO?> GetMovieDetailsAsync(int externalId);
    Task<bool> IsApiAvailableAsync();

    Task<ExternalMovieDetailDTO?> GetMovieDetailAsync(int externalId)
        => GetMovieDetailsAsync(externalId);
}
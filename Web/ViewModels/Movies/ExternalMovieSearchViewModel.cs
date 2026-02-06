namespace cnu_cinema_practice.ViewModels.Movies;

public class ExternalMovieSearchViewModel
{
    public int ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public int? Year { get; set; }
}
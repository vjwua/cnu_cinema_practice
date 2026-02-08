namespace cnu_cinema_practice.ViewModels.Movies;

public class ImportMoviePreviewViewModel
{
    public int ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public byte AgeLimit { get; set; }
    public string GenresText { get; set; } = string.Empty;
    public DateOnly? ReleaseDate { get; set; }
    public decimal? ImdbRating { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string? Country { get; set; }
    public List<string> Directors { get; set; } = new();
    public List<string> Actors { get; set; } = new();
}
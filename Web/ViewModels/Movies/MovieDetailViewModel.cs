namespace cnu_cinema_practice.ViewModels.Movies;

public class MovieDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string AgeLimit { get; set; } = string.Empty;
    public List<string> Genres { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string ImdbRating { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public string TrailerUrl { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public string FormattedDuration => $"{DurationMinutes / 60}h {DurationMinutes % 60}m";

    public string FormattedGenres => string.Join(", ", Genres);
}
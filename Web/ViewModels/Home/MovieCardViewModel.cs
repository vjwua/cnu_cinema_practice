namespace cnu_cinema_practice.ViewModels.Home;

/// <summary>
/// View model for movie cards displayed in grid or carousel
/// </summary>
public class MovieCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string? ImdbRating { get; set; }
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Description { get; set; } = string.Empty;

    public List<MovieSessionTimeViewModel> Sessions { get; set; } = new();

    public string FormattedDuration => $"{DurationMinutes / 60}h {DurationMinutes % 60}m";
}
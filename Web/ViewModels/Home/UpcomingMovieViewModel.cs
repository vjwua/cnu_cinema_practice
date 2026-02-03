namespace cnu_cinema_practice.ViewModels.Home;

/// <summary>
/// View model for upcoming movies (coming soon - not yet released or just released)
/// </summary>
public class UpcomingMovieViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string? Genre { get; set; }
    public decimal? ImdbRating { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public int DurationMinutes { get; set; }
    public string Description { get; set; } = string.Empty;

    public string FormattedDuration => $"{DurationMinutes / 60}h {DurationMinutes % 60}m";
    public string FormattedRating => ImdbRating?.ToString("F1") ?? "N/A";
}

namespace cnu_cinema_practice.ViewModels;

public class MovieCreateViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string PosterUrl { get; set; }
    public int DurationMinutes { get; set; }
    public List<string> Genres { get; set; }
    public string Rating { get; set; } // e.g., "PG-13", "R"
    public DateTime ReleaseDate { get; set; }

    public string FormattedDuration => $"{DurationMinutes / 60}h {DurationMinutes % 60}m";

    public string FormattedGenres => string.Join(", ", Genres);
}
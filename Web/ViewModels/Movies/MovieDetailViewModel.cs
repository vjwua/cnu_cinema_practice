namespace cnu_cinema_practice.ViewModels.Movies;

public class MovieDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DurationMinutes { get; set; }
    public string AgeLimit { get; set; }
    public List<string> Genres { get; set; }
    public string Description { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string ImdbRating { get; set; }
    public string PosterUrl { get; set; }
    public string TrailerUrl { get; set; }
    public string Director { get; set; }
    public string Country { get; set; }

    public string FormattedDuration => $"{DurationMinutes / 60}h {DurationMinutes % 60}m";

    public string FormattedGenres => string.Join(", ", Genres);
}
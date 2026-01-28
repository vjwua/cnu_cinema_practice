namespace cnu_cinema_practice.ViewModels.Home;

/// <summary>
/// View model for the home page (Index)
/// Contains featured movie, currently showing movies, and coming soon movies
/// </summary>
public class HomeIndexViewModel
{
    public MovieCardViewModel? FeaturedMovie { get; set; }
    public List<MovieCardViewModel> Movies { get; set; } = new();
    public List<UpcomingMovieViewModel> UpcomingMovies { get; set; } = new();
}

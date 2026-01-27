using cnu_cinema_practice.ViewModels.Home;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Controllers;

public class HomeController(IMovieService movieService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var moviesWithSessions = await movieService.GetAllWithUpcomingSessionsAsync();

        var movieCards = moviesWithSessions.Select(movie => new MovieCardViewModel
        {
            Id = movie.Id,
            Name = movie.Name,
            PosterUrl = movie.PosterUrl,
            ImdbRating = movie.ImdbRating?.ToString("F1"),
            Genre = movie.Genre.ToString(),
            DurationMinutes = movie.DurationMinutes,
            Sessions = movie.Sessions.Take(2).Select(s => new MovieSessionTimeViewModel
            {
                SessionId = s.Id,
                StartTime = s.StartTime
            }).ToList()
        }).ToList();

        // Get upcoming movies (coming soon - no sessions yet)
        var upcomingMovies = await movieService.GetUpcomingMoviesAsync();
        var upcomingMovieViewModels = upcomingMovies.Select(m => new UpcomingMovieViewModel
        {
            Id = m.Id,
            Name = m.Name,
            PosterUrl = m.PosterUrl,
            Genre = m.Genre.ToString(),
            ImdbRating = m.ImdbRating,
            ReleaseDate = m.ReleaseDate,
            DurationMinutes = m.DurationMinutes
        }).ToList();

        var viewModel = new HomeIndexViewModel
        {
            FeaturedMovie = movieCards.FirstOrDefault(),
            Movies = movieCards,
            UpcomingMovies = upcomingMovieViewModels
        };

        return View(viewModel);
    }
}
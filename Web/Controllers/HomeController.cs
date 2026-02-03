using cnu_cinema_practice.ViewModels.Home;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace cnu_cinema_practice.Controllers;

public class HomeController(IMovieService movieService) : Controller
{
    public async Task<IResult> Index()
    {
        var moviesWithSessions = await movieService.GetAllWithUpcomingSessionsAsync();
        // Filter out movies that have no upcoming sessions
        moviesWithSessions = moviesWithSessions.Where(m => m.Sessions.Any()).ToList();

        var movieCards = moviesWithSessions.Select(movie => new MovieCardViewModel
        {
            Id = movie.Id,
            Name = movie.Name,
            PosterUrl = movie.PosterUrl,
            ImdbRating = movie.ImdbRating?.ToString("F1"),
            Genre = movie.Genre.ToString(),
            DurationMinutes = movie.DurationMinutes,
            Description = movie.Description ?? string.Empty,
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
            DurationMinutes = m.DurationMinutes,
            Description = m.Description ?? string.Empty
        }).ToList();

        var viewModel = new HomeIndexViewModel
        {
            FeaturedMovie = movieCards.FirstOrDefault(),
            Movies = movieCards,
            UpcomingMovies = upcomingMovieViewModels
        };

        return new RazorComponentResult<cnu_cinema_practice.Components.Pages.Home>(new { Model = viewModel });
    }
}
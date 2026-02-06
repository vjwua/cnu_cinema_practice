using AutoMapper;
using cnu_cinema_practice.ViewModels.Home;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace cnu_cinema_practice.Controllers;

public class HomeController(IMovieService movieService, IMapper mapper) : Controller
{
    public async Task<IResult> Index()
    {
        var moviesWithSessions = await movieService.GetAllWithUpcomingSessionsAsync();
        moviesWithSessions = moviesWithSessions.Where(m => m.Sessions.Any()).ToList();

        var movieCards = mapper.Map<List<MovieCardViewModel>>(moviesWithSessions);

        var upcomingMovies = await movieService.GetUpcomingMoviesAsync();
        var upcomingMovieViewModels = mapper.Map<List<UpcomingMovieViewModel>>(upcomingMovies);

        var viewModel = new HomeIndexViewModel
        {
            FeaturedMovie = movieCards.FirstOrDefault(),
            Movies = movieCards,
            UpcomingMovies = upcomingMovieViewModels
        };

        return new RazorComponentResult<cnu_cinema_practice.Components.Pages.Home>(new { Model = viewModel });
    }
}
using cnu_cinema_practice.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var movies = new List<MovieOverviewViewModel>
            {
                new MovieOverviewViewModel
                {
                    Id = 1,
                    Name = "The Grand Adventure",
                    PosterUrl = "https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png",
                    DurationMinutes = 142,
                    Genres = new List<string> { "Action", "Adventure", "Sci-Fi" },
                    ImdbRating = "12+",
                    ReleaseDate = new DateTime(2026, 1, 15)
                },
                new MovieOverviewViewModel
                {
                    Id = 2,
                    Name = "Mystery at Midnight",
                    PosterUrl = "https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png",
                    DurationMinutes = 118,
                    Genres = new List<string> { "Mystery", "Thriller" },
                    ImdbRating = "18+",
                    ReleaseDate = new DateTime(2026, 1, 10)
                },
                new MovieOverviewViewModel
                {
                    Id = 3,
                    Name = "Comedy Central",
                    PosterUrl = "https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png",
                    DurationMinutes = 95,
                    Genres = new List<string> { "Comedy", "Romance" },
                    ImdbRating = "��",
                    ReleaseDate = new DateTime(2025, 12, 20)
                },
                new MovieOverviewViewModel
                {
                    Id = 4,
                    Name = "Drama Unfolds",
                    PosterUrl = "https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png",
                    DurationMinutes = 156,
                    Genres = new List<string> { "Drama", "Biography" },
                    ImdbRating = "16+",
                    ReleaseDate = new DateTime(2026, 1, 5)
                }
            };

        return View(movies);
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
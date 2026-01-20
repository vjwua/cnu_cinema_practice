using cnu_cinema_practice.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
// using cnu_cinema_practice.Models;

namespace cnu_cinema_practice.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var movies = new List<MovieCreateViewModel>
            {
                new MovieCreateViewModel
                {
                    Id = 1,
                    Title = "The Grand Adventure",
                    PosterUrl = "https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png",
                    DurationMinutes = 142,
                    Genres = new List<string> { "Action", "Adventure", "Sci-Fi" },
                    Rating = "12+",
                    ReleaseDate = new DateTime(2026, 1, 15)
                },
                new MovieCreateViewModel
                {
                    Id = 2,
                    Title = "Mystery at Midnight",
                    PosterUrl = "https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png",
                    DurationMinutes = 118,
                    Genres = new List<string> { "Mystery", "Thriller" },
                    Rating = "18+",
                    ReleaseDate = new DateTime(2026, 1, 10)
                },
                new MovieCreateViewModel
                {
                    Id = 3,
                    Title = "Comedy Central",
                    PosterUrl = "https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png",
                    DurationMinutes = 95,
                    Genres = new List<string> { "Comedy", "Romance" },
                    Rating = "ÇÀ",
                    ReleaseDate = new DateTime(2025, 12, 20)
                },
                new MovieCreateViewModel
                {
                    Id = 4,
                    Title = "Drama Unfolds",
                    PosterUrl = "https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png",
                    DurationMinutes = 156,
                    Genres = new List<string> { "Drama", "Biography" },
                    Rating = "16+",
                    ReleaseDate = new DateTime(2026, 1, 5)
                }
            };

        return View(movies);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Reverse()
    {
        return View();
    }
    // ...
}
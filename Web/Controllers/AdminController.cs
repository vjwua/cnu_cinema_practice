using cnu_cinema_practice.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Controllers
{
    public class AdminController : Controller
    {
        // Dashboard
        public IActionResult Index()
        {
            return View();
        }

        #region Movies Management

        // List all movies
        public IActionResult Movies()
        {
            // TODO: Fetch from database
            var movies = new List<AdminMovieViewModel>
            {
                new AdminMovieViewModel
                {
                    Id = 1,
                    Name = "The Grand Adventure",
                    PosterUrl = "https://via.placeholder.com/150x225/FF6B6B/FFFFFF?text=Movie+1",
                    DurationMinutes = 142,
                    ImdbRating = "12+",
                    ReleaseDate = new DateTime(2026, 1, 15),
                    Director = "John Smith",
                    Description = "An epic adventure across the galaxy.",
                    Genres = new List<string> { "Action", "Adventure", "Sci-Fi" },
                    IsActive = true
                },
                new AdminMovieViewModel
                {
                    Id = 2,
                    Name = "Mystery at Midnight",
                    PosterUrl = "https://via.placeholder.com/150x225/4ECDC4/FFFFFF?text=Movie+2",
                    DurationMinutes = 118,
                    ImdbRating = "18+",
                    ReleaseDate = new DateTime(2026, 1, 10),
                    Director = "Jane Doe",
                    Description = "A thrilling mystery that will keep you guessing.",
                    Genres = new List<string> { "Mystery", "Thriller" },
                    IsActive = true
                }
            };

            return View(movies);
        }

        // Create movie - GET
        public IActionResult CreateMovie()
        {
            return View(new MovieFormViewModel
            {
                ReleaseDate = DateTime.Now,
                IsActive = true
            });
        }

        // Create movie - POST
        [HttpPost]
        public IActionResult CreateMovie(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // TODO: Save to database
            TempData["Success"] = $"Movie '{model.Name}' created successfully!";
            return RedirectToAction("Movies");
        }

        // Edit movie - GET
        public IActionResult EditMovie(int id)
        {
            // TODO: Fetch from database
            var movie = new MovieFormViewModel
            {
                Id = 1,
                Name = "The Grand Adventure",
                PosterUrl = "https://via.placeholder.com/300x450/FF6B6B/FFFFFF?text=Movie+1",
                DurationMinutes = 142,
                ImdbRating = "12+",
                ReleaseDate = new DateTime(2026, 1, 15),
                Director = "John Smith",
                Description = "An epic adventure across the galaxy.",
                GenresString = "Action, Adventure, Sci-Fi",
                IsActive = true
            };

            return View(movie);
        }

        // Edit movie - POST
        [HttpPost]
        public IActionResult EditMovie(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // TODO: Update in database
            TempData["Success"] = $"Movie '{model.Name}' updated successfully!";
            return RedirectToAction("Movies");
        }

        // Delete movie
        [HttpPost]
        public IActionResult DeleteMovie(int id)
        {
            // TODO: Delete from database
            TempData["Success"] = "Movie deleted successfully!";
            return RedirectToAction("Movies");
        }

        // Toggle movie active status
        [HttpPost]
        public IActionResult ToggleMovieStatus(int id)
        {
            // TODO: Update status in database
            return RedirectToAction("Movies");
        }

        #endregion

        #region Hall Management

        // List all halls
        public IActionResult Halls()
        {
            // TODO: Fetch from database
            var halls = new List<AdminHallViewModel>
            {
                new AdminHallViewModel
                {
                    Id = 1,
                    Name = "Hall 1",
                    Rows = 8,
                    SeatsPerRow = 12,
                    IsAvailable = true
                },
                new AdminHallViewModel
                {
                    Id = 2,
                    Name = "Hall 2",
                    Rows = 10,
                    SeatsPerRow = 14,
                    IsAvailable = true
                },
                new AdminHallViewModel
                {
                    Id = 3,
                    Name = "VIP Hall",
                    Rows = 5,
                    SeatsPerRow = 8,
                    IsAvailable = false
                }
            };

            return View(halls);
        }

        // Create hall - GET
        public IActionResult CreateHall()
        {
            return View(new HallFormViewModel { IsActive = true });
        }

        // Create hall - POST
        [HttpPost]
        public IActionResult CreateHall(HallFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // TODO: Save to database
            TempData["Success"] = $"Hall '{model.Name}' created successfully!";
            return RedirectToAction("Halls");
        }

        // Edit hall - GET
        public IActionResult EditHall(int id)
        {
            // TODO: Fetch from database
            var hall = new HallFormViewModel
            {
                Id = 1,
                Name = "Hall 1",
                Rows = 8,
                SeatsPerRow = 12,
                IsActive = true
            };

            return View(hall);
        }

        // Edit hall - POST
        [HttpPost]
        public IActionResult EditHall(HallFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // TODO: Update in database
            TempData["Success"] = $"Hall '{model.Name}' updated successfully!";
            return RedirectToAction("Halls");
        }

        // Manage hall layout
        public IActionResult HallLayout(int id)
        {
            // TODO: Fetch from database
            var layout = new HallLayoutViewModel
            {
                HallId = id,
                HallName = "Hall 1",
                Rows = 8,
                SeatsPerRow = 12,
                DisabledSeats = new List<string> { "A1", "A2", "H11", "H12" } // Example disabled seats
            };

            return View(layout);
        }

        // Update hall layout
        [HttpPost]
        public IActionResult UpdateHallLayout(int hallId, string disabledSeats)
        {
            // TODO: Save disabled seats to database
            var seatList = string.IsNullOrEmpty(disabledSeats)
                ? new List<string>()
                : disabledSeats.Split(',').ToList();

            TempData["Success"] = $"Hall layout updated! {seatList.Count} seats disabled.";
            return RedirectToAction("Halls");
        }

        // Delete hall
        [HttpPost]
        public IActionResult DeleteHall(int id)
        {
            // TODO: Delete from database
            TempData["Success"] = "Hall deleted successfully!";
            return RedirectToAction("Halls");
        }

        #endregion
    }
}
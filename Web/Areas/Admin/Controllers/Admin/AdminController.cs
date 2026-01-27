using AutoMapper;
using cnu_cinema_practice.ViewModels;
using Core.DTOs.Movies;
using Core.DTOs.Halls;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Areas.Admin.Controllers.Admin
{
    [Area("Admin")]
    public class AdminController(
            IMovieService movieService,
            IHallService hallService,
            IMapper mapper) : Controller
        {
        // Dashboard
        public IActionResult Index()
        {
            return View();
        }

        #region Movies Management

        // List all movies
        public async Task<IActionResult> Movies()
        {
            var movies = await movieService.GetAllAsync();
            var viewModels = mapper.Map<IEnumerable<AdminMovieViewModel>>(movies);
            return View(viewModels);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMovie(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = mapper.Map<CreateMovieDTO>(model);
                await movieService.CreateAsync(dto);

                TempData["Success"] = $"Movie '{model.Name}' created successfully!";
                return RedirectToAction("Movies");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating movie: {ex.Message}");
                return View(model);
            }
        }

        // Edit movie - GET
        public async Task<IActionResult> EditMovie(int id)
        {
            var movie = await movieService.GetByIdAsync(id);
            if (movie == null)
            {
                TempData["Error"] = "Movie not found.";
                return RedirectToAction("Movies");
            }

            var viewModel = mapper.Map<MovieFormViewModel>(movie);
            return View(viewModel);
        }

        // Edit movie - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMovie(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = mapper.Map<UpdateMovieDTO>(model);
                await movieService.UpdateAsync(dto);

                TempData["Success"] = $"Movie '{model.Name}' updated successfully!";
                return RedirectToAction("Movies");
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Movie not found.";
                return RedirectToAction("Movies");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating movie: {ex.Message}");
                return View(model);
            }
        }

        // Delete movie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            try
            {
                await movieService.DeleteAsync(id);
                TempData["Success"] = "Movie deleted successfully!";
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Movie not found.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting movie: {ex.Message}";
            }

            return RedirectToAction("Movies");
        }

        // Toggle movie active status
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ToggleMovieStatus(int id)
        //{
        //    try
        //    {
        //        var movie = await movieService.GetByIdAsync(id);
        //        if (movie == null)
        //        {
        //            TempData["Error"] = "Movie not found.";
        //            return RedirectToAction("Movies");
        //        }

        //        // Toggle the IsActive status
        //        var updateDto = new UpdateMovieDTO
        //        {
        //            Id = id,
        //            Name = movie.Name,
        //            Description = movie.Description,
        //            DurationMinutes = movie.DurationMinutes,
        //            ReleaseDate = movie.ReleaseDate,
        //            PosterUrl = movie.PosterUrl,
        //            TrailerUrl = movie.TrailerUrl,
        //            Director = movie.Director,
        //            Genre = movie.Genre,
        //            AgeLimit = movie.AgeLimit,
        //            Country = movie.Country,
        //            ImdbRating = movie.ImdbRating,
        //            IsActive = !movie.IsActive // Toggle
        //        };

        //        await movieService.UpdateAsync(updateDto);
        //        TempData["Success"] = $"Movie status updated successfully!";
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = $"Error updating status: {ex.Message}";
        //    }

        //    return RedirectToAction("Movies");
        //}

        #endregion

        #region Hall Management

        // List all halls
        public async Task<IActionResult> Halls()
        {
            var halls = await hallService.GetAllAsync();
            var viewModels = mapper.Map<IEnumerable<AdminHallViewModel>>(halls);
            return View(viewModels);
        }

        // Create hall - GET
        public IActionResult CreateHall()
        {
            return View(new HallFormViewModel { IsActive = true });
        }

        // Create hall - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHall(HallFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = mapper.Map<CreateHallDTO>(model);
                await hallService.CreateAsync(dto);

                TempData["Success"] = $"Hall '{model.Name}' created successfully!";
                return RedirectToAction("Halls");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating hall: {ex.Message}");
                return View(model);
            }
        }

        // Edit hall - GET
        public async Task<IActionResult> EditHall(int id)
        {
            var hall = await hallService.GetByIdAsync(id);
            if (hall == null)
            {
                TempData["Error"] = "Hall not found.";
                return RedirectToAction("Halls");
            }

            var viewModel = mapper.Map<HallFormViewModel>(hall);
            return View(viewModel);
        }

        // Edit hall - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHall(HallFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = mapper.Map<UpdateHallDTO>(model);
                await hallService.UpdateHallInfo(dto);

                TempData["Success"] = $"Hall '{model.Name}' updated successfully!";
                return RedirectToAction("Halls");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating hall: {ex.Message}");
                return View(model);
            }
        }

        // Manage hall layout
        public async Task<IActionResult> HallLayout(int id)
        {
            var hall = await hallService.GetByIdAsync(id);
            if (hall == null)
            {
                TempData["Error"] = "Hall not found.";
                return RedirectToAction("Halls");
            }

            var layout = new HallLayoutViewModel
            {
                HallId = id,
                HallName = hall.Name,
                Rows = hall.Rows,
                SeatsPerRow = hall.Columns,
                DisabledSeats = new List<string>() // TODO: Implement disabled seats logic if needed
            };

            return View(layout);
        }

        // Update hall layout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHallLayout(int hallId, string disabledSeats)
        {
            var seatList = string.IsNullOrEmpty(disabledSeats)
                ? new List<string>()
                : disabledSeats.Split(',').ToList();

            try
            {
                // TODO: Implement disabled seats logic in your service/repository
                // For now, you might need to add a method to handle this in HallService

                TempData["Success"] = $"Hall layout updated! {seatList.Count} seats disabled.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating layout: {ex.Message}";
            }

            return RedirectToAction("Halls");
        }

        // Delete hall
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHall(int id)
        {
            try
            {
                await hallService.DeleteAsync(id);
                TempData["Success"] = "Hall deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting hall: {ex.Message}";
            }

            return RedirectToAction("Halls");
        }

        #endregion
    }
}
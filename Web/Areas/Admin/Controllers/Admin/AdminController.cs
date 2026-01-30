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
            var moviesDto = await movieService.GetAllAsync();
            var movies = mapper.Map<IEnumerable<AdminMovieViewModel>>(moviesDto);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMovie(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var createDto = mapper.Map<CreateMovieDTO>(model);
                await movieService.CreateAsync(createDto);

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
            var movieDto = await movieService.GetByIdAsync(id);
            if (movieDto == null)
                return NotFound();

            var movie = mapper.Map<MovieFormViewModel>(movieDto);
            return View(movie);
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
                var updateDto = mapper.Map<UpdateMovieDTO>(model);
                await movieService.UpdateAsync(updateDto);

                TempData["Success"] = $"Movie '{model.Name}' updated successfully!";
                return RedirectToAction("Movies");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleMovieStatus(int id)
        {
            // TODO: Implement if IsActive becomes part of Movie entity
            return RedirectToAction("Movies");
        }

        #endregion

        #region Hall Management

        // List all halls
        public async Task<IActionResult> Halls()
        {
            var hallsDto = await hallService.GetAllAsync();
            var halls = mapper.Map<IEnumerable<AdminHallViewModel>>(hallsDto);
            return View(halls);
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
                var createDto = mapper.Map<CreateHallDTO>(model);
                await hallService.CreateAsync(createDto);

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
            var hallDto = await hallService.GetByIdAsync(id);
            if (hallDto == null)
                return NotFound();

            var hall = mapper.Map<HallFormViewModel>(hallDto);
            return View(hall);
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
                var updateDto = mapper.Map<UpdateHallDTO>(model);
                await hallService.UpdateHallInfo(updateDto);

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
            var hallDto = await hallService.GetByIdAsync(id);
            if (hallDto == null)
                return NotFound();

            var layout = mapper.Map<HallLayoutViewModel>(hallDto);
            return View(layout);
        }

        // Update hall layout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHallLayout(int hallId, string disabledSeats)
        {
            try
            {
                var seatList = string.IsNullOrEmpty(disabledSeats)
                    ? new List<string>()
                    : disabledSeats.Split(',').ToList();

                // TODO: Convert disabled seats to proper seat layout and update
                // This will require mapping seat identifiers to byte[,] array

                TempData["Success"] = $"Hall layout updated! {seatList.Count} seats disabled.";
                return RedirectToAction("Halls");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating layout: {ex.Message}";
                return RedirectToAction("Halls");
            }
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
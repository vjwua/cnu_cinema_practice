using AutoMapper;
using cnu_cinema_practice.ViewModels.Movies;
using Core.DTOs.Movies;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MovieController(
        IMovieService movieService,
        IMapper mapper) : Controller
    {
        // List all movies
        public async Task<IActionResult> Index()
        {
            var movies = await movieService.GetAllAsync();
            var viewModels = mapper.Map<IEnumerable<MovieListViewModel>>(movies);
            return View(viewModels);
        }

        // Create movie - GET
        public IActionResult Create()
        {
            return View(new CreateMovieViewModel
            {
                ReleaseDate = DateTime.Now
            });
        }

        // Create movie - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMovieViewModel model)
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
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating movie: {ex.Message}");
                return View(model);
            }
        }

        // Edit movie - GET
        public async Task<IActionResult> Edit(int id)
        {
            var movieDto = await movieService.GetByIdAsync(id);
            if (movieDto == null)
                return NotFound();

            var movie = mapper.Map<UpdateMovieViewModel>(movieDto);
            return View(movie);
        }

        // Edit movie - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateMovieViewModel model)
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
                return RedirectToAction("Index");
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
        public async Task<IActionResult> Delete(int id)
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

            return RedirectToAction("Index");
        }

        // Toggle movie active status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            // TODO: Implement if IsActive becomes part of Movie entity
            return RedirectToAction("Index");
        }
    }
}
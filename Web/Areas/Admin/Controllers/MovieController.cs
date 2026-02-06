using AutoMapper;
using cnu_cinema_practice.ViewModels.Movies;
using Core.DTOs.Movies;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace cnu_cinema_practice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MovieController(
        IMovieService movieService,
        IExternalMovieService externalMovieService,
        IMapper mapper,
        ILogger<MovieController> logger) : Controller
    {
        private static readonly ConcurrentDictionary<string, Queue<DateTime>> SearchRateLimiter = new();

        public async Task<IActionResult> Index()
        {
            var movies = await movieService.GetAllAsync();
            var viewModels = mapper.Map<IEnumerable<MovieListViewModel>>(movies);
            return View(viewModels);
        }

        public IActionResult Create()
        {
            return View(new CreateMovieViewModel
            {
                ReleaseDate = DateTime.Now
            });
        }

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

        public async Task<IActionResult> Edit(int id)
        {
            var movieDto = await movieService.GetByIdAsync(id);
            if (movieDto == null)
                return NotFound();

            var movie = mapper.Map<UpdateMovieViewModel>(movieDto);
            return View(movie);
        }

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

        [HttpGet]
        public async Task<IActionResult> ExternalApiStatus()
        {
            var available = await externalMovieService.IsApiAvailableAsync();
            return Json(new { available });
        }

        [HttpGet]
        public async Task<IActionResult> SearchExternal(string query)
        {
            if (!AllowSearchRequest())
                return StatusCode(429);

            if (string.IsNullOrWhiteSpace(query))
                return PartialView("_ExternalSearchPartial", Enumerable.Empty<ExternalMovieSearchViewModel>());

            var available = await externalMovieService.IsApiAvailableAsync();
            if (!available)
                return PartialView("_ExternalSearchPartial", Enumerable.Empty<ExternalMovieSearchViewModel>());

            var results = await externalMovieService.SearchMoviesAsync(query);
            var vms = mapper.Map<IEnumerable<ExternalMovieSearchViewModel>>(results);

            return PartialView("_ExternalSearchPartial", vms);
        }

        [HttpGet]
        public async Task<IActionResult> ImportPreview(int id)
        {
            var available = await externalMovieService.IsApiAvailableAsync();
            if (!available)
                return Content(string.Empty);

            var dto = await externalMovieService.GetMovieDetailsAsync(id);
            if (dto == null)
                return Content(string.Empty);

            var vm = mapper.Map<ImportMoviePreviewViewModel>(dto);
            return PartialView("_ImportPreviewPartial", vm);
        }

        public sealed class ImportRequest
        {
            public int ExternalId { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExternal([FromBody] ImportRequest request)
        {
            var available = await externalMovieService.IsApiAvailableAsync();
            if (!available)
                return Json(new { success = false, message = "External API is unavailable." });

            var dto = await externalMovieService.GetMovieDetailsAsync(request.ExternalId);
            if (dto == null)
                return Json(new { success = false, message = "Movie not found." });

            var director = dto.Directors.FirstOrDefault() ?? string.Empty;

            return Json(new
            {
                success = true,
                movie = new
                {
                    name = dto.Name,
                    description = dto.Description ?? string.Empty,
                    durationMinutes = dto.DurationMinutes,
                    ageLimit = dto.AgeLimit,
                    genresText = dto.GenresText,
                    releaseDate = dto.ReleaseDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    imdbRating = dto.ImdbRating,
                    posterUrl = dto.PosterUrl ?? string.Empty,
                    trailerUrl = dto.TrailerUrl ?? string.Empty,
                    country = dto.Country ?? string.Empty,
                    director = director
                }
            });
        }

        private bool AllowSearchRequest()
        {
            try
            {
                const int windowSeconds = 60;
                const int maxRequests = 30;

                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var now = DateTime.UtcNow;

                var queue = SearchRateLimiter.GetOrAdd(ip, _ => new Queue<DateTime>());
                lock (queue)
                {
                    while (queue.Count > 0 && (now - queue.Peek()).TotalSeconds > windowSeconds)
                        queue.Dequeue();

                    if (queue.Count >= maxRequests)
                        return false;

                    queue.Enqueue(now);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Rate limiter failed");
                return true;
            }
        }
    }
}

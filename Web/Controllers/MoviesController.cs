using Microsoft.AspNetCore.Mvc;
using Core.Interfaces.Services;
using Core.DTOs.Movies;
using Core.Enums;
using cnu_cinema_practice.ViewModels;

namespace cnu_cinema_practice.Controllers;

public class MoviesController : Controller
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    public async Task<IActionResult> Index()
    {
        var movies = await _movieService.GetAllAsync();
        return View(movies);
    }

    public async Task<IActionResult> Details(int id)
    {
        var movie = await _movieService.GetByIdAsync(id);
        if (movie == null)
            return NotFound();
        return View(movie);
    }

    public async Task<IActionResult> SearchByName(string name)
    {
        var movies = await _movieService.SearchByNameAsync(name);
        return View(movies);
    }

    public async Task<IActionResult> SearchByGenre(MovieGenre genre)
    {
        var movies = await _movieService.GetByGenreAsync(genre);
        return View(movies);
    }

    //admin

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMovieDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }
        
        await  _movieService.CreateAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var movie = await _movieService.GetByIdAsync(id);
        if (movie == null)
            return NotFound();

        var dto = new UpdateMovieDTO()
        {
            Id = movie.Id,
            Name = movie.Name,
            Description = movie.Description,
            DurationMinutes = movie.DurationMinutes,
            Genre = movie.Genre,
            ReleaseDate = movie.ReleaseDate
        };
        
        return View(movie);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateMovieDTO dto)
    {
        if(!ModelState.IsValid)
            return View(dto);
        
        await _movieService.UpdateAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _movieService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
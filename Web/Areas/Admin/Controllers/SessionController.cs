using AutoMapper;
using cnu_cinema_practice.ViewModels.Halls;
using cnu_cinema_practice.ViewModels.Sessions;
using Core.Constants;
using Core.DTOs.Sessions;
using Core.Enums;
using Core.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace cnu_cinema_practice.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public class SessionController(
    ISessionService sessionService,
    IMovieService movieService,
    IHallService hallService,
    IMapper mapper) : Controller
{
    #region Public Actions

    public async Task<IActionResult> Index(string? search, string? format, string? dateFilter, int page = 1)
    {
        var viewModel = await BuildIndexViewModelAsync(search, format, dateFilter, page);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> IndexResults(string? search, string? format, string? dateFilter, int page = 1)
    {
        var viewModel = await BuildIndexViewModelAsync(search, format, dateFilter, page);
        return PartialView("_SessionIndexResults", viewModel);
    }

    public async Task<IActionResult> Schedule(DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;

        var startDate = selectedDate.Date;
        var endDate = startDate.AddDays(7);

        var sessions = await sessionService.GetSessionsByDateRangeAsync(startDate, endDate);
        var halls = await hallService.GetAllAsync();

        var viewModel = new SessionScheduleViewModel
        {
            SelectedDate = selectedDate,
            StartDate = startDate,
            EndDate = endDate,
            Sessions = mapper.Map<IEnumerable<SessionViewModel>>(sessions),
            Halls = mapper.Map<IEnumerable<HallListViewModel>>(halls)
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var session = await sessionService.GetSessionByIdWithSeatsAsync(id);
        if (session == null)
            return NotFound();

        var viewModel = mapper.Map<SessionDetailsViewModel>(session);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(DateTime? startTime, int? hallId, int? movieId)
    {
        await PopulateDropdownsAsync();

        var model = new CreateSessionViewModel();

        if (startTime.HasValue)
            model.StartTime = startTime.Value;

        if (hallId.HasValue)
            model.HallId = hallId.Value;

        if (movieId.HasValue)
            model.MovieId = movieId.Value;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSessionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        try
        {
            var dto = mapper.Map<CreateSessionDTO>(model);
            await sessionService.CreateSessionAsync(dto);

            TempData["SuccessMessage"] = "Session created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            HandleValidationException(ex);
        }
        catch (InvalidOperationException ex)
        {
            HandleBusinessException(ex);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
        }

        await PopulateDropdownsAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var session = await sessionService.GetSessionByIdAsync(id);
        if (session == null)
            return NotFound();

        var viewModel = mapper.Map<UpdateSessionViewModel>(session);
        await PopulateDropdownsAsync();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateSessionViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        try
        {
            var dto = mapper.Map<UpdateSessionDTO>(model);
            await sessionService.UpdateSessionAsync(id, dto);

            TempData["SuccessMessage"] = "Session updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            HandleValidationException(ex);
        }
        catch (InvalidOperationException ex)
        {
            HandleBusinessException(ex);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
        }

        await PopulateDropdownsAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await sessionService.DeleteSessionAsync(id);
            TempData["SuccessMessage"] = "Session deleted successfully!";
        }
        catch (ValidationException ex)
        {
            TempData["ErrorMessage"] = $"Validation error: {ex.Message}";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = $"Operation error: {ex.Message}";
        }
        catch (DbUpdateException ex)
        {
            TempData["ErrorMessage"] =
                "Cannot delete this session because it has associated bookings or reservations. Please cancel all bookings first.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An unexpected error occurred while deleting the session: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CheckConflict(
        int hallId,
        DateTime startTime,
        int movieId,
        int? excludeSessionId = null)
    {
        if (hallId <= 0 || movieId <= 0)
            return Json(new { hasConflict = false });

        try
        {
            var movie = await movieService.GetByIdAsync(movieId);
            if (movie == null)
                return Json(new { hasConflict = false });

            var hasConflict = await sessionService.HasScheduleConflictAsync(
                hallId,
                startTime,
                movie.DurationMinutes,
                excludeSessionId);

            return Json(new
            {
                hasConflict,
                message = hasConflict
                    ? "Schedule conflict detected. The hall is already occupied at this time."
                    : "No conflict detected."
            });
        }
        catch (InvalidOperationException)
        {
            return Json(new { hasConflict = false, message = "Error checking conflict." });
        }
    }

    #endregion

    #region Private Helper Methods

    private void HandleValidationException(ValidationException ex)
    {
        foreach (var error in ex.Errors)
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
    }

    private void HandleBusinessException(InvalidOperationException ex)
    {
        ModelState.AddModelError(string.Empty, ex.Message);
    }

    private async Task PopulateDropdownsAsync()
    {
        var movies = await movieService.GetAllAsync();
        var halls = await hallService.GetAllAsync();

        ViewBag.Movies = new SelectList(movies, "Id", "Name");
        ViewBag.Halls = new SelectList(halls, "Id", "Name");
    }

    private async Task<SessionIndexViewModel> BuildIndexViewModelAsync(
        string? search,
        string? format,
        string? dateFilter,
        int page)
    {
        var parsedFormat = ParseMovieFormat(format);

        var result = await sessionService.GetAdminPagedAsync(new SessionAdminQueryDTO
        {
            Search = search,
            MovieFormat = parsedFormat,
            DateFilter = dateFilter,
            Page = page <= 0 ? 1 : page
        });

        return new SessionIndexViewModel
        {
            Paged = new Core.DTOs.Common.PagedResult<SessionViewModel>
            {
                Items = result.Items.Select(mapper.Map<SessionViewModel>).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            },
            Search = search,
            Format = format,
            DateFilter = dateFilter
        };
    }

    private static MovieFormat? ParseMovieFormat(string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
            return null;

        var normalized = format.Trim();

        return normalized.ToUpperInvariant() switch
        {
            "2D" => MovieFormat.TwoD,
            "3D" => MovieFormat.ThreeD,
            "IMAX" => MovieFormat.IMAX,
            "4DX" => MovieFormat.FourDX,
            _ => Enum.TryParse<MovieFormat>(normalized, ignoreCase: true, out var parsed) ? parsed : null
        };
    }

    #endregion
}
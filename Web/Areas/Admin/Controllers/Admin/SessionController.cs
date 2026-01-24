using AutoMapper;
using cnu_cinema_practice.ViewModels.Sessions;
using Core.DTOs.Sessions;
using Core.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace cnu_cinema_practice.Areas.Admin.Controllers.Admin;

[Area("Admin")]
public class SessionsController(
    ISessionService sessionService,
    IMovieService movieService,
    IHallService hallService,
    IMapper mapper) : Controller
{
    #region Public Actions

    public async Task<IActionResult> Index()
    {
        var sessions = await sessionService.GetAllSessionsAsync();
        var viewModels = mapper.Map<IEnumerable<AdminSessionViewModel>>(sessions);
        return View(viewModels);
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
    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View(new CreateSessionViewModel());
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
            await PopulateDropdownsAsync();
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            HandleBusinessException(ex);
            await PopulateDropdownsAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var session = await sessionService.GetSessionByIdAsync(id);
        if (session == null)
            return NotFound();

        var viewModel = mapper.Map<EditSessionViewModel>(session);
        await PopulateDropdownsAsync();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditSessionViewModel model)
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
            await PopulateDropdownsAsync();
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            HandleBusinessException(ex);
            await PopulateDropdownsAsync();
            return View(model);
        }
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
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    //can be used for checking for sessions conflicts in real time (using AJAX)
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
        catch (Exception)
        {
            return Json(new { hasConflict = false });
        }
    }

    #endregion

    #region Private Helper Methods

    private void HandleValidationException(ValidationException ex)
    {
        foreach (var error in ex.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
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

    #endregion
}